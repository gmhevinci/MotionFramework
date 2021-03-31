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
	internal sealed class AssetDatabaseSubProvider : AssetProviderBase
	{
		public override float Progress
		{
			get
			{
				if (IsDone)
					return 100f;
				else
					return 0;
			}
		}

		public AssetDatabaseSubProvider(AssetLoaderBase owner, string assetName, System.Type assetType)
			: base(owner, assetName, assetType)
		{
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (States == EAssetStates.None)
			{
				States = EAssetStates.Loading;
			}

			// 1. 加载资源对象
			if (States == EAssetStates.Loading)
			{
				string assetPath = Owner.BundleInfo.LocalPath;
				var findAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);
				List<UnityEngine.Object> result = new List<Object>(findAssets.Length);
				foreach (var findObj in findAssets)
				{
					if (findObj.GetType() == AssetType)
						result.Add(findObj);
				}
				AllAssets = result.ToArray();
				States = EAssetStates.Checking;
			}

			// 2. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				States = AllAssets == null ? EAssetStates.Fail : EAssetStates.Success;
				if (States == EAssetStates.Fail)
					MotionLog.Warning($"Failed to load all asset object : {Owner.BundleInfo.LocalPath}");
				InvokeCompletion();
			}
#endif
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