//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Network;

namespace MotionFramework.Resource
{
	internal sealed class AssetBundleLoader : FileLoaderBase
	{
		private readonly List<AssetBundleLoader> _masters = new List<AssetBundleLoader>(10);
		private readonly List<AssetBundleLoader> _depends = new List<AssetBundleLoader>(10);
		private WebFileRequest _downloader;
		private AssetBundleCreateRequest _cacheRequest;
		private bool _isWaitForAsyncComplete = false;
		internal AssetBundle CacheBundle { private set; get; }

		public AssetBundleLoader(AssetBundleInfo bundleInfo)
			: base(bundleInfo)
		{
			// 准备依赖列表
			string[] dependencies = AssetSystem.BundleServices.GetDirectDependencies(bundleInfo.BundleName);
			if (dependencies != null && dependencies.Length > 0)
			{
				foreach (string dependBundleName in dependencies)
				{
					AssetBundleInfo dependBundleInfo = AssetSystem.BundleServices.GetAssetBundleInfo(dependBundleName);
					AssetBundleLoader dependLoader = AssetSystem.CreateLoaderInternal(dependBundleInfo) as AssetBundleLoader;
					dependLoader.AddMaster(this);
					_depends.Add(dependLoader);
				}
			}
		}
		public override void Update()
		{
			// 如果资源文件加载完毕
			if (States == ELoaderStates.Success || States == ELoaderStates.Fail)
			{
				UpdateProviders();
				return;
			}

			if (States == ELoaderStates.None)
			{
				// 检测加载地址是否为空
				if (string.IsNullOrEmpty(BundleInfo.LocalPath))
				{
					States = ELoaderStates.Fail;
					return;
				}

				if (string.IsNullOrEmpty(BundleInfo.RemoteURL))
					States = ELoaderStates.CheckDepends;
				else
					States = ELoaderStates.Download;
			}

			// 1. 从服务器下载
			if (States == ELoaderStates.Download)
			{
				int failedTryAgain = 3;
				_downloader = WebFileSystem.GetWebFileRequest(BundleInfo.RemoteURL, BundleInfo.RemoteFallbackURL, BundleInfo.LocalPath, failedTryAgain);
				States = ELoaderStates.CheckDownload;
			}

			// 2. 检测服务器下载结果
			if (States == ELoaderStates.CheckDownload)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_downloader.ReportError();
					States = ELoaderStates.Fail;
				}
				else
				{
					// 校验文件完整性
					if (AssetSystem.BundleServices.CheckContentIntegrity(BundleInfo.BundleName) == false)
					{
						MotionLog.Error($"Check download content integrity is failed : {BundleInfo.BundleName}");
						States = ELoaderStates.Fail;
					}
					else
					{
						States = ELoaderStates.CheckDepends;
					}
				}

				// 释放网络资源下载器
				if (_downloader != null)
				{
					_downloader.Dispose();
					_downloader = null;
				}
			}

			// 3. 检测所有依赖完成状态
			if (States == ELoaderStates.CheckDepends)
			{
				foreach (var dpLoader in _depends)
				{
					if (_isWaitForAsyncComplete)
						dpLoader.WaitForAsyncComplete();

					if (dpLoader.CehckFileLoadDone() == false)
						return;
				}
				States = ELoaderStates.LoadFile;
			}

			// 4. 加载AssetBundle
			if (States == ELoaderStates.LoadFile)
			{
#if UNITY_EDITOR
				// 注意：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
				if (System.IO.File.Exists(BundleInfo.LocalPath) == false)
				{
					MotionLog.Warning($"Not found assetBundle file : {BundleInfo.LocalPath}");
					States = ELoaderStates.Fail;
					return;
				}
#endif

				// Load assetBundle file
				if (BundleInfo.IsEncrypted)
				{
					if (AssetSystem.DecryptServices == null)
						throw new Exception($"{nameof(AssetBundleLoader)} need IDecryptServices : {BundleInfo.BundleName}");

					EDecryptMethod decryptType = AssetSystem.DecryptServices.DecryptType;
					if (decryptType == EDecryptMethod.GetDecryptOffset)
					{
						ulong offset = AssetSystem.DecryptServices.GetDecryptOffset(BundleInfo);
						if (_isWaitForAsyncComplete)
							CacheBundle = AssetBundle.LoadFromFile(BundleInfo.LocalPath, 0, offset);
						else
							_cacheRequest = AssetBundle.LoadFromFileAsync(BundleInfo.LocalPath, 0, offset);
					}
					else if (decryptType == EDecryptMethod.GetDecryptBinary)
					{
						byte[] binary = AssetSystem.DecryptServices.GetDecryptBinary(BundleInfo);
						if (_isWaitForAsyncComplete)
							CacheBundle = AssetBundle.LoadFromMemory(binary);
						else
							_cacheRequest = AssetBundle.LoadFromMemoryAsync(binary);
					}
					else
					{
						throw new NotImplementedException($"{decryptType}");
					}
				}
				else
				{
					if (_isWaitForAsyncComplete)
						CacheBundle = AssetBundle.LoadFromFile(BundleInfo.LocalPath);
					else
						_cacheRequest = AssetBundle.LoadFromFileAsync(BundleInfo.LocalPath);
				}
				States = ELoaderStates.CheckFile;
			}

			// 5. 检测AssetBundle加载结果
			if (States == ELoaderStates.CheckFile)
			{
				if (_cacheRequest != null)
				{
					if (_isWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						CacheBundle = _cacheRequest.assetBundle;
					}
					else
					{
						if (_cacheRequest.isDone == false)
							return;
						CacheBundle = _cacheRequest.assetBundle;
					}
				}

				// Check error
				if (CacheBundle == null)
				{
					MotionLog.Warning($"Failed to load assetBundle file : {BundleInfo.BundleName}");
					States = ELoaderStates.Fail;
				}
				else
				{
					States = ELoaderStates.Success;
				}
			}
		}
		public override void Reference()
		{
			base.Reference();

			// 同时引用一遍所有依赖资源
			for (int i = 0; i < _depends.Count; i++)
			{
				_depends[i].Reference();
			}
		}
		public override void Release()
		{
			base.Release();

			// 同时释放一遍所有依赖资源
			for (int i = 0; i < _depends.Count; i++)
			{
				_depends[i].Release();
			}
		}
		public override void Destroy(bool checkFatal)
		{
			base.Destroy(checkFatal);

			// Check fatal
			if (checkFatal)
			{
				if (RefCount > 0)
					throw new Exception($"Bundle file loader ref is not zero : {BundleInfo.BundleName}");
				if (IsDone() == false)
					throw new Exception($"Bundle file loader is not done : {BundleInfo.BundleName}");
			}

			if (_downloader != null)
			{
				_downloader.Dispose();
				_downloader = null;
			}

			if (CacheBundle != null)
			{
				CacheBundle.Unload(true);
				CacheBundle = null;
			}

			foreach(var dependLoader in _depends)
			{
				dependLoader.RemoveMaster(this);
			}

			_depends.Clear();
			_masters.Clear();
		}
		public override bool CanDestroy()
		{
			if (base.CanDestroy() == false)
				return false;

			// 注意：我们必须等待主资源已经可以销毁的时候，才可以销毁依赖资源
			// 在一些特殊情况下：
			// 当依赖资源被销毁的时候，而主资源因为还未加载完毕而暂时不能销毁，
			// 当再次使用主资源的时候，因为依赖资源已经销毁导致实例化的资源不完整。
			foreach (var masterLoader in _masters)
			{
				if (masterLoader.IsDestroyed)
					continue;
				if (masterLoader.CanDestroy() == false)
					return false;
			}
			return true;
		}
		public override void WaitForAsyncComplete()
		{
			if (IsSceneLoader)
			{
				MotionLog.Warning($"Scene is not support {nameof(WaitForAsyncComplete)}.");
				return;
			}

			_isWaitForAsyncComplete = true;

			int frame = 1000;
			while (true)
			{
				// 保险机制
				// 注意：如果需要从WEB端下载资源，可能会触发保险机制！
				frame--;
				if (frame == 0)
					throw new Exception($"Should never get here ! BundleName : {BundleInfo.BundleName} States : {States}");

				// 驱动流程
				Update();

				// 完成后退出
				if (IsDone())
					break;
			}
		}

		public void AddMaster(AssetBundleLoader master)
		{
#if UNITY_EDITOR
			foreach (var loader in _masters)
			{
				if (loader == master)
					throw new Exception("Should never get here.");
			}
#endif

			_masters.Add(master);
		}
		public void RemoveMaster(AssetBundleLoader master)
		{
#if UNITY_EDITOR
			bool exist = false;
			foreach (var loader in _masters)
			{
				if (loader == master)
				{
					exist = true;
					break;
				}
			}
			if (exist == false)
				throw new Exception("Should never get here.");
#endif

			for (int i = _masters.Count - 1; i >= 0; i--)
			{
				if (_masters[i] == master)
				{
					_masters.RemoveAt(i);
					break;
				}
			}
		}
	}
}