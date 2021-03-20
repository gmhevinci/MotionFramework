//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Resource
{
	internal sealed class AssetDatabaseLoader : AssetLoaderBase
	{
		public AssetDatabaseLoader(AssetBundleInfo bundleInfo)
			: base(bundleInfo)
		{
		}
		public override void Update()
		{
#if UNITY_EDITOR
			// 如果资源文件加载完毕
			if (States == ELoaderStates.Fail || States == ELoaderStates.Success)
			{
				UpdateAllProvider();
				return;
			}

			// 检测资源文件是否存在
			string guid = UnityEditor.AssetDatabase.AssetPathToGUID(BundleInfo.LocalPath);
			if (string.IsNullOrEmpty(guid))
				States = ELoaderStates.Fail;
			else
				States = ELoaderStates.Success;
#endif
		}
		public override void ForceSyncLoad()
		{
#if UNITY_EDITOR
			if (IsSceneLoader)
				return;

			int frame = 1000;
			while (true)
			{
				// 保险机制
				frame--;
				if (frame == 0)
					throw new Exception($"Should never get here ! {BundleInfo.BundleName} = {States}");

				// 更新加载流程
				Update();

				// 完成后退出
				if (IsDone())
					break;
			}
#endif
		}
	}
}
