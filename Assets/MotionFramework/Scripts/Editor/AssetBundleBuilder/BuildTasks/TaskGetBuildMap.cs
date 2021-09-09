//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MotionFramework.Patch;

namespace MotionFramework.Editor
{
	public class TaskGetBuildMap : IBuildTask
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
			/// 获取AssetBundle内包含的标记列表
			/// </summary>
			public string[] GetAssetTags(string bundleFullName)
			{
				if (TryGetBundleInfo(bundleFullName, out BundleInfo bundleInfo))
				{
					return bundleInfo.GetAssetTags();
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

			/// <summary>
			/// 获取AssetBundle内构建的资源路径列表
			/// </summary>
			public string[] GetBuildinAssetPaths(string bundleFullName)
			{
				if (TryGetBundleInfo(bundleFullName, out BundleInfo bundleInfo))
				{
					return bundleInfo.GetBuildinAssetPaths();
				}
				throw new Exception($"Not found {nameof(BundleInfo)} : {bundleFullName}");
			}

			/// <summary>
			/// 获取构建管线里需要的数据
			/// </summary>
			public UnityEditor.AssetBundleBuild[] GetPipelineBuilds()
			{
				List<AssetBundleBuild> builds = new List<AssetBundleBuild>(BundleInfos.Count);
				foreach (var bundleInfo in BundleInfos)
				{
					if (bundleInfo.IsRawFile == false)
						builds.Add(bundleInfo.CreatePipelineBuild());
				}
				return builds.ToArray();
			}

			/// <summary>
			/// 检测是否包含BundleName
			/// </summary>
			public bool IsContainsBundle(string bundleFullName)
			{
				return TryGetBundleInfo(bundleFullName, out BundleInfo bundleInfo);
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

			// 检测构建结果
			CheckBuildMapContent(buildMapContext);
		}

		/// <summary>
		/// 获取构建的资源列表
		/// </summary>
		private List<AssetInfo> GetBuildAssets()
		{
			Dictionary<string, AssetInfo> buildAssets = new Dictionary<string, AssetInfo>();

			// 1. 获取主动收集的资源
			List<AssetCollectInfo> allCollectInfos = AssetBundleCollectorSettingData.GetAllCollectAssets();

			// 2. 对收集的资源进行依赖分析
			int progressValue = 0;
			foreach (AssetCollectInfo collectInfo in allCollectInfos)
			{
				string mainAssetPath = collectInfo.AssetPath;

				// 获取所有依赖资源
				List<AssetInfo> depends = GetAllDependencies(mainAssetPath);
				for (int i = 0; i < depends.Count; i++)
				{
					string assetPath = depends[i].AssetPath;

					// 如果已经存在，则增加该资源的依赖计数
					if (buildAssets.ContainsKey(assetPath))
					{
						buildAssets[assetPath].DependCount++;
					}
					else
					{
						buildAssets.Add(assetPath, depends[i]);
					}

					// 添加资源标记
					buildAssets[assetPath].AddAssetTags(collectInfo.AssetTags);

					// 注意：检测是否为主动收集资源
					if (assetPath == mainAssetPath)
					{
						buildAssets[assetPath].IsCollectAsset = true;
						buildAssets[assetPath].IsRawAsset = collectInfo.IsRawAsset;
					}
				}

				// 添加所有的依赖资源列表
				// 注意：不包括自己
				var allDependAssetInfos = new List<AssetInfo>(depends.Count);
				for (int i = 0; i < depends.Count; i++)
				{
					string assetPath = depends[i].AssetPath;
					if (assetPath != mainAssetPath)
						allDependAssetInfos.Add(buildAssets[assetPath]);
				}
				buildAssets[mainAssetPath].SetAllDependAssetInfos(allDependAssetInfos);

				EditorTools.DisplayProgressBar("依赖文件分析", ++progressValue, allCollectInfos.Count);
			}
			EditorTools.ClearProgressBar();

			// 3. 设置资源包名
			progressValue = 0;
			foreach (KeyValuePair<string, AssetInfo> pair in buildAssets)
			{
				var assetInfo = pair.Value;
				var bundleLabel = AssetBundleCollectorSettingData.GetBundleLabel(assetInfo.AssetPath);
				if(assetInfo.IsRawAsset)
					assetInfo.SetBundleLabelAndVariant(bundleLabel, PatchDefine.RawFileVariant);
				else
					assetInfo.SetBundleLabelAndVariant(bundleLabel, PatchDefine.AssetBundleFileVariant);
				EditorTools.DisplayProgressBar("设置资源包名", ++progressValue, buildAssets.Count);
			}
			EditorTools.ClearProgressBar();

			// 4. 返回结果
			return buildAssets.Values.ToList();
		}

		/// <summary>
		/// 获取指定资源依赖的所有资源列表
		/// 注意：返回列表里已经包括主资源自己
		/// </summary>
		private List<AssetInfo> GetAllDependencies(string mainAssetPath)
		{
			List<AssetInfo> result = new List<AssetInfo>();
			string[] depends = AssetDatabase.GetDependencies(mainAssetPath, true);
			foreach (string assetPath in depends)
			{
				if (AssetBundleCollectorSettingData.IsValidateAsset(assetPath))
				{
					AssetInfo assetInfo = new AssetInfo(assetPath);
					result.Add(assetInfo);
				}
			}
			return result;
		}

		/// <summary>
		/// 检测构建结果
		/// </summary>
		private void CheckBuildMapContent(BuildMapContext buildMapContext)
		{
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				// 注意：原生文件资源包只能包含一个原生文件
				bool isRawFile = bundleInfo.IsRawFile;
				if (isRawFile)
				{			
					if (bundleInfo.Assets.Count != 1)
						throw new Exception("Should never get here !");
					continue;
				}

				// 注意：原生文件不能被其它资源文件依赖
				foreach (var assetInfo in bundleInfo.Assets)
				{				
					if (assetInfo.AllDependAssetInfos != null)
					{
						foreach (var dependAssetInfo in assetInfo.AllDependAssetInfos)
						{						
							if (dependAssetInfo.IsRawAsset)
								throw new Exception($"{assetInfo.AssetPath} can not depend raw asset : {dependAssetInfo.AssetPath}");
						}
					}
				}
			}
		}
	}
}