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
			public readonly List<BundleBuildInfo> BuildInfos = new List<BundleBuildInfo>();

			/// <summary>
			/// 添加一个打包资源
			/// </summary>
			public void PackAsset(AssetInfo assetInfo)
			{
				if (TryGetBundleBuildInfo(assetInfo.GetAssetBundleFullName(), out BundleBuildInfo buildInfo))
				{
					buildInfo.PackAsset(assetInfo);
				}
				else
				{
					BundleBuildInfo newBuildInfo = new BundleBuildInfo(assetInfo.AssetBundleLabel, assetInfo.AssetBundleVariant);
					newBuildInfo.PackAsset(assetInfo);
					BuildInfos.Add(newBuildInfo);
				}
			}

			/// <summary>
			/// 获取所有的打包资源
			/// </summary>
			public List<AssetInfo> GetAllAssets()
			{
				List<AssetInfo> result = new List<AssetInfo>(BuildInfos.Count);
				foreach (var buildInfo in BuildInfos)
				{
					result.AddRange(buildInfo.Assets);
				}
				return result;
			}

			/// <summary>
			/// 获取构建管线里需要的数据
			/// </summary>
			public UnityEditor.AssetBundleBuild[] GetPipelineBuilds()
			{
				List<AssetBundleBuild> builds = new List<AssetBundleBuild>(BuildInfos.Count);
				for (int i = 0; i < BuildInfos.Count; i++)
				{
					BundleBuildInfo buildInfo = BuildInfos[i];
					builds.Add(buildInfo.CreateAssetBundleBuild());
				}
				return builds.ToArray();
			}

			/// <summary>
			/// 检测AssetBundle的收集标记
			/// </summary>
			public bool IsCollectBundle(string bundleFullName)
			{
				if (TryGetBundleBuildInfo(bundleFullName, out BundleBuildInfo buildInfo))
				{
					return buildInfo.IsCollectBundle;
				}
				throw new Exception($"Not found {nameof(BundleBuildInfo)} : {bundleFullName}");
			}

			/// <summary>
			/// 获取AssetBundle内包含的资源路径列表
			/// </summary>
			public string[] GetAssetPaths(string bundleFullName)
			{
				if (TryGetBundleBuildInfo(bundleFullName, out BundleBuildInfo buildInfo))
				{
					return buildInfo.GetAssetPaths();
				}
				throw new Exception($"Not found {nameof(BundleBuildInfo)} : {bundleFullName}");
			}

			private bool TryGetBundleBuildInfo(string bundleFullName, out BundleBuildInfo result)
			{
				foreach (var buildInfo in BuildInfos)
				{
					if (buildInfo.AssetBundleFullName == bundleFullName)
					{
						result = buildInfo;
						return true;
					}
				}
				result = null;
				return false;
			}
		}

		void IBuildTask.Run(BuildContext context)
		{
			List<AssetInfo> allAssets = GetBuildAssets();
			if (allAssets.Count == 0)
				throw new Exception("构建的资源列表不能为空");

			BuildLogger.Log($"构建的资源列表里总共有{allAssets.Count}个资源");
			BuildMapContext buildMapContext = new BuildMapContext();
			foreach (var assetInfo in allAssets)
			{
				buildMapContext.PackAsset(assetInfo);
			}
			context.SetContextObject(buildMapContext);
		}

		/// <summary>
		/// 获取构建的资源列表
		/// </summary>
		private List<AssetInfo> GetBuildAssets()
		{
			int progressBarCount = 0;
			Dictionary<string, AssetInfo> buildAssets = new Dictionary<string, AssetInfo>();

			// 获取要收集的资源
			List<string> allCollectAssets = AssetBundleCollectorSettingData.GetAllCollectAssets();

			// 进行依赖分析
			foreach (string mainAssetPath in allCollectAssets)
			{
				List<AssetInfo> depends = GetDependencies(mainAssetPath);
				for (int i = 0; i < depends.Count; i++)
				{
					AssetInfo assetInfo = depends[i];
					if (buildAssets.ContainsKey(assetInfo.AssetPath))
						buildAssets[assetInfo.AssetPath].DependCount++;
					else
						buildAssets.Add(assetInfo.AssetPath, assetInfo);
				}
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"依赖文件分析：{progressBarCount}/{allCollectAssets.Count}", (float)progressBarCount / allCollectAssets.Count);
			}
			EditorUtility.ClearProgressBar();
			progressBarCount = 0;

			// 移除零依赖的资源
			List<string> removeList = new List<string>();
			foreach (KeyValuePair<string, AssetInfo> pair in buildAssets)
			{
				if (pair.Value.IsCollectAsset)
					continue;
				if (pair.Value.DependCount == 0)
					removeList.Add(pair.Value.AssetPath);
			}
			for (int i = 0; i < removeList.Count; i++)
			{
				buildAssets.Remove(removeList[i]);
			}

			// 设置资源标签和变种
			foreach (KeyValuePair<string, AssetInfo> pair in buildAssets)
			{
				var assetInfo = pair.Value;
				var bundleLabelAndVariant = AssetBundleCollectorSettingData.GetBundleLabelAndVariant(assetInfo.AssetPath, assetInfo.AssetType);
				assetInfo.AssetBundleLabel = bundleLabelAndVariant.BundleLabel;
				assetInfo.AssetBundleVariant = bundleLabelAndVariant.BundleVariant;
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"设置资源标签：{progressBarCount}/{buildAssets.Count}", (float)progressBarCount / buildAssets.Count);
			}
			EditorUtility.ClearProgressBar();

			// 返回结果
			return buildAssets.Values.ToList();
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