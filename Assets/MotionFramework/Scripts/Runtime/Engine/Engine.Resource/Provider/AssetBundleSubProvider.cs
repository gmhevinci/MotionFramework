﻿//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Resource
{
	internal sealed class AssetBundleSubProvider : AssetProviderBase
	{
		private AssetBundleLoader _loader;
		private AssetBundleRequest _cacheRequest;
		public override float Progress
		{
			get
			{
				if (_cacheRequest == null)
					return 0;
				return _cacheRequest.progress;
			}
		}

		public AssetBundleSubProvider(FileLoaderBase owner, string assetName, System.Type assetType)
			: base(owner, assetName, assetType)
		{
			_loader = owner as AssetBundleLoader;
		}
		public override void Update()
		{
			if (IsDone)
				return;

			if (_loader.CacheBundle == null)
			{
				States = EAssetStates.Fail;
				InvokeCompletion();
			}

			if (States == EAssetStates.None)
			{
				States = EAssetStates.Loading;
			}

			// 1. 加载资源对象
			if (States == EAssetStates.Loading)
			{
				if (SyncLoadMode)
				{
					if (AssetType == null)
						AllAssets = _loader.CacheBundle.LoadAssetWithSubAssets(AssetName);
					else
						AllAssets = _loader.CacheBundle.LoadAssetWithSubAssets(AssetName, AssetType);
				}
				else
				{
					if (AssetType == null)
						_cacheRequest = _loader.CacheBundle.LoadAssetWithSubAssetsAsync(AssetName);
					else
						_cacheRequest = _loader.CacheBundle.LoadAssetWithSubAssetsAsync(AssetName, AssetType);
				}
				States = EAssetStates.Checking;
			}

			// 2. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				if (_cacheRequest != null)
				{
					if (SyncLoadMode)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						AllAssets = _cacheRequest.allAssets;
					}
					else
					{
						if (_cacheRequest.isDone == false)
							return;
						AllAssets = _cacheRequest.allAssets;
					}
				}

				States = AllAssets == null ? EAssetStates.Fail : EAssetStates.Success;
				if (States == EAssetStates.Fail)
					MotionLog.Warning($"Failed to load sub assets : {AssetName} from bundle : {_loader.BundleInfo.BundleName}");
				InvokeCompletion();
			}
		}
		public override void Destory()
		{
			base.Destory();

			if (AllAssets != null)
			{
				foreach (var assetObject in AllAssets)
				{
					if (assetObject is GameObject == false)
						Resources.UnloadAsset(assetObject);
				}
			}
		}
	}
}