//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Resource
{
	internal sealed class AssetBundleProvider : AssetProviderBase
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

		public AssetBundleProvider(FileLoaderBase owner, string assetName, System.Type assetType)
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
				if (IsWaitForAsyncComplete)
				{
					if (AssetType == null)
						AssetObject = _loader.CacheBundle.LoadAsset(AssetName);
					else
						AssetObject = _loader.CacheBundle.LoadAsset(AssetName, AssetType);
				}
				else
				{
					if (AssetType == null)
						_cacheRequest = _loader.CacheBundle.LoadAssetAsync(AssetName);
					else
						_cacheRequest = _loader.CacheBundle.LoadAssetAsync(AssetName, AssetType);
				}
				States = EAssetStates.Checking;
			}

			// 2. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				if (_cacheRequest != null)
				{
					if (IsWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						AssetObject = _cacheRequest.asset;
					}
					else
					{
						if (_cacheRequest.isDone == false)
							return;
						AssetObject = _cacheRequest.asset;
					}
				}

				States = AssetObject == null ? EAssetStates.Fail : EAssetStates.Success;
				if (States == EAssetStates.Fail)
					MotionLog.Warning($"Failed to load asset : {AssetName} from bundle : {_loader.BundleInfo.BundleName}");
				InvokeCompletion();
			}
		}
		public override void Destory()
		{
			base.Destory();

			if (AssetObject != null)
			{
				if (AssetObject is GameObject == false)
					Resources.UnloadAsset(AssetObject);
			}
		}
	}
}