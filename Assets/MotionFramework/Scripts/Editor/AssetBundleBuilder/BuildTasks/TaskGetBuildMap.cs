//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MotionFramework.Editor
{
	internal class TaskGetBuildMap : IBuildTask
	{
		public class BuildMapContext : IContextObject
		{
			public List<AssetInfo> BuildList;
			public AssetBundleBuild[] Builds;
		}

		void IBuildTask.Run(BuildContext context)
		{
			List<AssetInfo> buildMap = GetBuildMap();
			if (buildMap.Count == 0)
				throw new Exception("构建列表不能为空");

			BuildLogger.Log($"构建列表里总共有{buildMap.Count}个资源需要构建");
			List<AssetBundleBuild> builds = new List<AssetBundleBuild>(buildMap.Count);
			for (int i = 0; i < buildMap.Count; i++)
			{
				AssetInfo assetInfo = buildMap[i];
				AssetBundleBuild buildInfo = new AssetBundleBuild();
				buildInfo.assetBundleName = assetInfo.AssetBundleLabel;
				buildInfo.assetBundleVariant = assetInfo.AssetBundleVariant;
				buildInfo.assetNames = new string[] { assetInfo.AssetPath };
				builds.Add(buildInfo);
			}

			BuildMapContext buildMapContext = new BuildMapContext();
			buildMapContext.BuildList = buildMap;
			buildMapContext.Builds = builds.ToArray();
			context.SetContextObject(buildMapContext);
		}

		/// <summary>
		/// 获取构建的资源列表
		/// </summary>
		private List<AssetInfo> GetBuildMap()
		{
			int progressBarCount = 0;
			Dictionary<string, AssetInfo> buildMap = new Dictionary<string, AssetInfo>();

			// 获取要收集的资源
			List<string> allCollectAssets = AssetBundleCollectorSettingData.GetAllCollectAssets();

			// 进行依赖分析
			foreach (string mainAssetPath in allCollectAssets)
			{
				List<AssetInfo> depends = GetDependencies(mainAssetPath);
				for (int i = 0; i < depends.Count; i++)
				{
					AssetInfo assetInfo = depends[i];
					if (buildMap.ContainsKey(assetInfo.AssetPath))
						buildMap[assetInfo.AssetPath].DependCount++;
					else
						buildMap.Add(assetInfo.AssetPath, assetInfo);
				}
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"依赖文件分析：{progressBarCount}/{allCollectAssets.Count}", (float)progressBarCount / allCollectAssets.Count);
			}
			EditorUtility.ClearProgressBar();
			progressBarCount = 0;

			// 移除零依赖的资源
			List<string> removeList = new List<string>();
			foreach (KeyValuePair<string, AssetInfo> pair in buildMap)
			{
				if (pair.Value.IsCollectAsset)
					continue;
				if (pair.Value.DependCount == 0)
					removeList.Add(pair.Value.AssetPath);
			}
			for (int i = 0; i < removeList.Count; i++)
			{
				buildMap.Remove(removeList[i]);
			}

			// 设置资源标签
			foreach (KeyValuePair<string, AssetInfo> pair in buildMap)
			{
				var assetInfo = pair.Value;
				var bundleBuildInfo = AssetBundleCollectorSettingData.GetBundleBuildInfo(assetInfo.AssetPath, assetInfo.AssetType);
				assetInfo.AssetBundleLabel = bundleBuildInfo.BundleLabel;
				assetInfo.AssetBundleVariant = bundleBuildInfo.BundleVariant;
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"设置资源标签：{progressBarCount}/{buildMap.Count}", (float)progressBarCount / buildMap.Count);
			}
			EditorUtility.ClearProgressBar();

			// 返回结果
			return buildMap.Values.ToList();
		}

		/// <summary>
		/// 获取指定资源依赖的资源列表
		/// 注意：返回列表里已经包括主资源自己
		/// </summary>
		private List<AssetInfo> GetDependencies(string assetPath)
		{
			List<AssetInfo> depends = new List<AssetInfo>();
			string[] dependArray = AssetDatabase.GetDependencies(assetPath, true);
			foreach (string dependPath in dependArray)
			{
				if (AssetBundleCollectorSettingData.ValidateAsset(dependPath))
				{
					AssetInfo assetInfo = new AssetInfo(dependPath);
					depends.Add(assetInfo);
				}
			}
			return depends;
		}
	}
}