﻿//--------------------------------------------------
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
	internal sealed class AssetDatabaseLoader : FileLoaderBase
	{
		public AssetDatabaseLoader(AssetBundleInfo bundleInfo)
			: base(bundleInfo)
		{
		}
		public override void Update()
		{
#if UNITY_EDITOR
			base.Update();

			// 如果资源文件加载完毕
			if (CheckFileLoadDone())
				return;

			if (States == ELoaderStates.None)
			{
				// 检测资源文件是否存在
				string guid = UnityEditor.AssetDatabase.AssetPathToGUID(BundleInfo.LocalPath);
				if (string.IsNullOrEmpty(guid))
					States = ELoaderStates.Fail;
				else
					States = ELoaderStates.Success;
			}
#endif
		}
		public override void WaitForAsyncComplete()
		{
#if UNITY_EDITOR
			if (IsSceneLoader)
			{
				MotionLog.Warning($"Scene is not support {nameof(WaitForAsyncComplete)}.");
				return;
			}

			int frame = 1000;
			while (true)
			{
				// 保险机制
				frame--;
				if (frame == 0)
					throw new Exception($"Should never get here ! BundleName : {BundleInfo.BundleName} States : {States}");

				// 驱动流程
				Update();

				// 完成后退出
				if (IsDone())
					break;
			}
#endif
		}
	}
}
