//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
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
					States = ELoaderStates.LoadDepends;
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
					if (AssetSystem.BundleServices.CheckContentIntegrity(BundleInfo.ManifestPath) == false)
					{
						MotionLog.Error($"Check download content integrity is failed : {BundleInfo.ManifestPath}");
						States = ELoaderStates.Fail;
					}
					else
					{
						States = ELoaderStates.LoadDepends;
					}
				}

				// 释放网络资源下载器
				if (_downloader != null)
				{
					_downloader.Dispose();
					_downloader = null;
				}
			}

			// 3. 加载所有依赖项
			if (States == ELoaderStates.LoadDepends)
			{
				string[] dependencies = AssetSystem.BundleServices.GetDirectDependencies(BundleInfo.ManifestPath);
				if (dependencies != null && dependencies.Length > 0)
				{
					foreach (string dpManifestPath in dependencies)
					{
						AssetBundleInfo dpBundleInfo = AssetSystem.BundleServices.GetAssetBundleInfo(dpManifestPath);
						AssetLoaderBase dpLoader = AssetSystem.CreateLoaderInternal(dpBundleInfo);
						_depends.Add(dpLoader);
					}
				}
				States = ELoaderStates.CheckDepends;
			}

			// 4. 检测所有依赖完成状态
			if (States == ELoaderStates.CheckDepends)
			{
				foreach (var dpLoader in _depends)
				{
					if (dpLoader.IsDone() == false)
						return;
				}
				States = ELoaderStates.LoadFile;
			}

			// 5. 加载AssetBundle
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
						throw new Exception($"AssetBundle need IDecryptServices : {BundleInfo.ManifestPath}");

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

			// 6. 检测AssetBundle加载结果
			if (States == ELoaderStates.CheckFile)
			{
				if (_cacheRequest.isDone == false)
					return;
				CacheBundle = _cacheRequest.assetBundle;

				// Check error
				if (CacheBundle == null)
				{
					MotionLog.Warning($"Failed to load assetBundle file : {BundleInfo.LocalPath}");
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
				throw new Exception($"Bundle file loader ref is not zero : {BundleInfo.LocalPath}");
			if (IsDone() == false)
				throw new Exception($"Bundle file loader is not done : {BundleInfo.LocalPath}");

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
		public override bool IsDone()
		{
			if (base.IsDone() == false)
				return false;

			return CheckAllProviderIsDone();
		}
	}
}