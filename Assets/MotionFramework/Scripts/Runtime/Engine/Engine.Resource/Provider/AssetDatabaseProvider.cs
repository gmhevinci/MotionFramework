//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Resource
{
	internal sealed class AssetDatabaseProvider : AssetProviderBase
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

		public AssetDatabaseProvider(AssetLoaderBase owner, string assetName, System.Type assetType)
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
				string loadPath = _owner.LoadPath;

				// 注意：如果加载路径指向的是文件夹
				if (UnityEditor.AssetDatabase.IsValidFolder(loadPath))
				{
					string folderPath = loadPath;
					string fileName = AssetName;
					loadPath = AssetPathHelper.FindDatabaseAssetPath(folderPath, fileName);
				}

				AssetObject = UnityEditor.AssetDatabase.LoadAssetAtPath(loadPath, AssetType);
				States = EAssetStates.Checking;
			}

			// 2. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				States = AssetObject == null ? EAssetStates.Fail : EAssetStates.Success;
				if (States == EAssetStates.Fail)
					MotionLog.Warning($"Failed to load asset object : {_owner.LoadPath} : {AssetName}");
				InvokeCompletion();
			}
#endif
		}
	}
}