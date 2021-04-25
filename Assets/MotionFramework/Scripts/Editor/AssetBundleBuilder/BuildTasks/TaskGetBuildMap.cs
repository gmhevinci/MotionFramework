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
			public readonly List<BundleInfo> BundleInfos = new List<BundleInfo>();

			/// <summary>
			/// 添加一个打包资源
			/// </summary>
			public void PackAsset(AssetInfo assetInfo)
			{
				if (TryGetBundleInfo(assetInfo.GetAssetBundleFullName(), out BundleInfo bundleInfo))
				{
					bundleInfo.PackAsset(assetInfo);
				}
				else
				{
					BundleInfo newBundleInfo = new BundleInfo(assetInfo.AssetBundleLabel, assetInfo.AssetBundleVariant);
					newBundleInfo.PackAsset(assetInfo);
					BundleInfos.Add(newBundleInfo);
				}
			}

			/// <summary>
			/// 获取所有的打包资源
			/// </summary>
			public List<AssetInfo> GetAllAssets()
			{
				List<AssetInfo> result = new List<AssetInfo>(BundleInfos.Count);
				foreach (var bundleInfo in BundleInfos)
				{
					result.AddRange(bundleInfo.Assets);
				}
				return result;
			}

			/// <summary>
			/// 获取构建管线里需要的数据
			/// </summary>
			public UnityEditor.AssetBundleBuild[] GetPipelineBuilds()
			{
				List<AssetBundleBuild> builds = new List<AssetBundleBuild>(BundleInfos.Count);
				foreach(var bundleInfo in BundleInfos)
				{
					builds.Add(bundleInfo.CreateAssetBundleBuild());
				}
				return builds.ToArray();
			}

			/// <summary>
			/// 获取AssetBundle内包含的资源路径列表
			/// </summary>
			public string[] GetIncludeAssetPaths(string bundleFullName)
			{
				if (TryGetBundleInfo(bundleFullName, out BundleInfo bundleInfo))
				{
					return bundleInfo.GetIncludeAssetPaths();
				}
				throw new Exception($"Not found {nameof(BundleInfo)} : {bundleFullName}");
			}

			/// <summary>
			/// 获取AssetBundle内收集的资源路径列表
			/// </summary>
			public string[] GetCollectAssetPaths(string bundleFullName)
			{
				if (TryGetBundleInfo(bundleFullName, out BundleInfo bundleInfo))
				{
					return bundleInfo.GetCollectAssetPaths();
				}
				throw new Exception($"Not found {nameof(BundleInfo)} : {bundleFullName}");
			}

			private bool TryGetBundleInfo(string bundleFullName, out BundleInfo result)
			{
				foreach (var bundleInfo in BundleInfos)
				{
					if (bundleInfo.AssetBundleFullName == bundleFullName)
					{
						result = bundleInfo;
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

			// 1. 获取主动收集的资源
			List<string> allCollectAssets = AssetBundleCollectorSettingData.GetAllCollectAssets();

			// 2. 对收集的资源进行依赖分析
			foreach (string collectAssetPath in allCollectAssets)
			{
				List<AssetInfo> depends = GetDependencies(collectAssetPath);
				for (int i = 0; i < depends.Count; i++)
				{
					AssetInfo assetInfo = depends[i];
					if (buildAssets.ContainsKey(assetInfo.AssetPath))
						buildAssets[assetInfo.AssetPath].DependCount++;
					else
						buildAssets.Add(assetInfo.AssetPath, assetInfo);

					// 注意：检测是否为主动收集资源
					if (assetInfo.AssetPath == collectAssetPath)
					{
						buildAssets[collectAssetPath].IsCollectAsset = true;
					}
				}
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"依赖文件分析：{progressBarCount}/{allCollectAssets.Count}", (float)progressBarCount / allCollectAssets.Count);
			}
			progressBarCount = 0;
			EditorUtility.ClearProgressBar();		

			// 3. 移除零依赖的资源
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

			// 4. 设置资源标签和变种
			foreach (KeyValuePair<string, AssetInfo> pair in buildAssets)
			{
				var assetInfo = pair.Value;
				var bundleLabelAndVariant = AssetBundleCollectorSettingData.GetBundleLabelAndVariant(assetInfo.AssetPath, assetInfo.AssetType);
				assetInfo.SetBundleLabelAndVariant(bundleLabelAndVariant.BundleLabel, bundleLabelAndVariant.BundleVariant);
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"设置资源标签：{progressBarCount}/{buildAssets.Count}", (float)progressBarCount / buildAssets.Count);
			}
			EditorUtility.ClearProgressBar();

			// 5. 返回结果
			return buildAssets.Values.ToList();
		}

		/// <summary>
		/// 获取指定资源依赖的资源列表
		/// 注意：返回列表里已经包括主资源自己
		/// </summary>
		private List<AssetInfo> GetDependencies(string assetPath)
		{
			List<AssetInfo> result = new List<AssetInfo>();
			string[] depends= AssetDatabase.GetDependencies(assetPath, true);
			foreach (string dependAssetPath in depends)
			{
				if (AssetBundleCollectorSettingData.IsValidateAsset(dependAssetPath))
				{
					AssetInfo assetInfo = new AssetInfo(dependAssetPath);
					result.Add(assetInfo);
				}
			}
			return result;
		}
	}
}