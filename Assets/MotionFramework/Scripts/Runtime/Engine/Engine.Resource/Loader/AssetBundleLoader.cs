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
	internal sealed class AssetBundleLoader : AssetLoaderBase
	{
		private readonly List<AssetLoaderBase> _depends = new List<AssetLoaderBase>(10);
		private WebFileRequest _downloader;
		private AssetBundleCreateRequest _cacheRequest;
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
					AssetLoaderBase dependLoader = AssetSystem.CreateLoaderInternal(dependBundleInfo);
					_depends.Add(dependLoader);
				}
			}
		}
		public override void Update()
		{
			// 如果资源文件加载完毕
			if (States == ELoaderStates.Success || States == ELoaderStates.Fail)
			{
				UpdateAllProvider();
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
				_downloader = new WebFileRequest(BundleInfo.RemoteURL, BundleInfo.LocalPath);
				_downloader.DownLoad();
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
					if (dpLoader.IsDone() == false)
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
						_cacheRequest = AssetBundle.LoadFromFileAsync(BundleInfo.LocalPath, 0, offset);
					}
					else if (decryptType == EDecryptMethod.GetDecryptBinary)
					{
						byte[] binary = AssetSystem.DecryptServices.GetDecryptBinary(BundleInfo);
						_cacheRequest = AssetBundle.LoadFromMemoryAsync(binary);
					}
					else
					{
						throw new NotImplementedException($"{decryptType}");
					}
				}
				else
				{
					_cacheRequest = AssetBundle.LoadFromFileAsync(BundleInfo.LocalPath);
				}
				States = ELoaderStates.CheckFile;
			}

			// 5. 检测AssetBundle加载结果
			if (States == ELoaderStates.CheckFile)
			{
				if (_cacheRequest.isDone == false)
					return;
				CacheBundle = _cacheRequest.assetBundle;

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
		public override void Destroy(bool force)
		{
			base.Destroy(force);

			// Check fatal
			if (RefCount > 0)
				throw new Exception($"Bundle file loader ref is not zero : {BundleInfo.BundleName}");
			if (IsDone() == false)
				throw new Exception($"Bundle file loader is not done : {BundleInfo.BundleName}");

			if (_downloader != null)
			{
				_downloader.Dispose();
				_downloader = null;
			}

			if (CacheBundle != null)
			{
				CacheBundle.Unload(force);
				CacheBundle = null;
			}

			_depends.Clear();
		}
		public override void ForceSyncLoad()
		{
			if (IsSceneLoader)
				return;

			int frame = 1000;
			while (true)
			{
				// 保险机制
				// 注意：如果需要从WEB端下载资源，可能会触发保险机制！
				frame--;
				if (frame == 0)
					throw new Exception($"Should never get here ! {BundleInfo.BundleName} = {States}");

				// 更新加载流程
				Update();

				// 强制加载依赖文件
				if (States == ELoaderStates.CheckDepends)
				{
					foreach (var dpLoader in _depends)
					{
						dpLoader.ForceSyncLoad();
					}
				}

				// 挂起主线程
				if (States == ELoaderStates.CheckFile)
				{
					CacheBundle = _cacheRequest.assetBundle;
				}

				// 强制加载资源对象
				if (States == ELoaderStates.Success || States == ELoaderStates.Fail)
				{
					for (int i = 0; i < _providers.Count; i++)
					{
						var provider = _providers[i] as AssetProviderBase;
						provider.ForceSyncLoad();
					}
				}

				// 完成后退出
				if (IsDone())
					break;
			}
		}
	}
}