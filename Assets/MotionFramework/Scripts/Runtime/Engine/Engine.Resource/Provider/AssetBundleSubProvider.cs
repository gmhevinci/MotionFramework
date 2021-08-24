//--------------------------------------------------
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
		private BundleFileGrouper _bundleGrouper;
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

		public AssetBundleSubProvider(string assetPath, System.Type assetType)
			: base(assetPath, assetType)
		{
			_bundleGrouper = new BundleFileGrouper(assetPath);
		}
		public override void Update()
		{
			if (IsDone)
				return;

			if (States == EAssetStates.None)
			{
				States = EAssetStates.CheckBundle;
			}

			// 1. 检测资源包
			if (States == EAssetStates.CheckBundle)
			{
				if (IsWaitForAsyncComplete)
					_bundleGrouper.WaitForAsyncComplete();

				if (_bundleGrouper.IsDone() == false)
					return;

				if (_bundleGrouper.OwnerAssetBundle == null)
				{
					States = EAssetStates.Fail;
					InvokeCompletion();
				}
				else
				{
					States = EAssetStates.Loading;
				}
			}

			// 2. 加载资源对象
			if (States == EAssetStates.Loading)
			{
				if (IsWaitForAsyncComplete)
				{
					if (AssetType == null)
						AllAssets = _bundleGrouper.OwnerAssetBundle.LoadAssetWithSubAssets(AssetName);
					else
						AllAssets = _bundleGrouper.OwnerAssetBundle.LoadAssetWithSubAssets(AssetName, AssetType);
				}
				else
				{
					if (AssetType == null)
						_cacheRequest = _bundleGrouper.OwnerAssetBundle.LoadAssetWithSubAssetsAsync(AssetName);
					else
						_cacheRequest = _bundleGrouper.OwnerAssetBundle.LoadAssetWithSubAssetsAsync(AssetName, AssetType);
				}
				States = EAssetStates.Checking;
			}

			// 3. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				if (_cacheRequest != null)
				{
					if (IsWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						MotionLog.Warning("Suspend the main thread to load unity asset.");
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
					MotionLog.Warning($"Failed to load sub assets : {AssetName} from bundle : {_bundleGrouper.OwnerBundleInfo.BundleName}");
				InvokeCompletion();
			}
		}
		public override void Destory()
		{
			base.Destory();

			// 销毁资源对象
			if (AllAssets != null)
			{
				foreach (var assetObject in AllAssets)
				{
					if (assetObject is GameObject == false)
						Resources.UnloadAsset(assetObject);
				}
			}

			// 释放资源包
			if (_bundleGrouper != null)
			{
				_bundleGrouper.Release();
				_bundleGrouper = null;
			}
		}
	}
}