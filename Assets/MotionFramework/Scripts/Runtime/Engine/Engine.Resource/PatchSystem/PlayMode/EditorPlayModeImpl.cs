//--------------------------------------------------
// Motion Framework
// Copyright©2022-2022 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Resource
{
	public sealed class EditorPlayModeImpl : IBundleServices
	{
		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync()
		{
			var operation = new EditorModeInitializationOperation();
			OperationUpdater.ProcessOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			return 0;
		}

		/// <summary>
		/// 获取内置资源标记列表
		/// </summary>
		public string[] GetManifestBuildinTags()
		{
			return new string[0];
		}

		#region IBundleServices接口
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string bundleName)
		{
			MotionLog.Warning($"Editor play mode can not get asset bundle info.");
			AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, bundleName);
			return bundleInfo;
		}
		bool IBundleServices.ContainsAsset(string assetPath)
		{
			return AssetPathHelper.ContainsDatabaseAsset(assetPath);
		}
		string IBundleServices.GetAssetBundleName(string assetPath)
		{
			return assetPath;
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return new string[] { };
		}
		#endregion
	}
}