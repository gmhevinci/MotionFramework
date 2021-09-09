//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MotionFramework.Editor
{
	public class TaskBuilding : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParametersContext = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();

			BuildLogger.Log($"开始构建......");
			BuildAssetBundleOptions opt = buildParametersContext.GetPiplineBuildOptions();
			AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(buildParametersContext.PipelineOutputDirectory, buildMapContext.GetPipelineBuilds(), opt, buildParametersContext.Parameters.BuildTarget);
			if (unityManifest == null)
				throw new Exception("构建过程中发生错误！");

			// 拷贝原生文件
			foreach (var bundleInfo in buildMapContext.BundleInfos)
			{
				if (bundleInfo.IsRawFile)
				{
					string dest = $"{buildParametersContext.PipelineOutputDirectory}/{bundleInfo.BundleName}";
					foreach(var buildAsset in bundleInfo.Assets)
					{
						if(buildAsset.IsRawAsset)
							EditorTools.CopyFile(buildAsset.AssetPath, dest, true);
					}
				}
			}

			// 验证构建结果
			if (buildParametersContext.Parameters.IsVerifyBuildingResult)
			{
				VerifyingBuildingResult(context, unityManifest);
			}
		}

		/// <summary>
		/// 验证构建结果
		/// </summary>
		private void VerifyingBuildingResult(BuildContext context, AssetBundleManifest unityManifest)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();

			// 1. 过滤掉原生Bundle
			List<BuildBundleInfo> buildBundleInfos = new List<BuildBundleInfo>(allAssetBundles.Length);
			foreach(var bundleInfo in buildMapContext.BundleInfos)
			{
				if (bundleInfo.IsRawFile == false)
					buildBundleInfos.Add(bundleInfo);
			}

			// 2. 验证数量		
			if (allAssetBundles.Length != buildBundleInfos.Count)
			{
				BuildLogger.Warning($"构建过程中可能发现了无效的资源，导致Bundle数量不一致！");
			}

			// 3. 正向验证Bundle
			foreach (var bundleName in allAssetBundles)
			{
				if (buildMapContext.IsContainsBundle(bundleName) == false)
				{
					throw new Exception($"Should never get here !");
				}
			}

			// 4. 反向验证Bundle
			foreach (var bundleInfo in buildBundleInfos)
			{
				bool isMatch = false;
				foreach (var bundleName in allAssetBundles)
				{
					if (bundleName == bundleInfo.BundleName)
					{
						isMatch = true;
						break;
					}
				}
				if (isMatch == false)
					throw new Exception($"无效的Bundle文件 : {bundleInfo.BundleName}");
			}

			// 5. 验证Asset
			int progressValue = 0;
			foreach (var bundleName in allAssetBundles)
			{
				string filePath = $"{buildParameters.PipelineOutputDirectory}/{bundleName}";

				string[] allAssetNames = GetAssetBundleAllAssets(filePath);
				string[] buildinAssetPaths = buildMapContext.GetBuildinAssetPaths(bundleName);
				if (buildinAssetPaths.Length != allAssetNames.Length)
					throw new Exception($"Should never get here !");

				foreach (var assetName in allAssetNames)
				{
					var guid = AssetDatabase.AssetPathToGUID(assetName);
					if (string.IsNullOrEmpty(guid))
						throw new Exception($"无效的资源路径，请检查路径是否带有特殊符号或中文：{assetName}");

					bool isMatch = false;
					foreach (var buildinAssetPath in buildinAssetPaths)
					{
						var guidTemp = AssetDatabase.AssetPathToGUID(buildinAssetPath);
						if (guid == guidTemp)
						{
							isMatch = true;
							break;
						}
					}
					if (isMatch == false)
						throw new Exception($"Should never get here !");
				}

				EditorTools.DisplayProgressBar("验证构建结果", ++progressValue, allAssetBundles.Length);
			}
			EditorTools.ClearProgressBar();

			// 卸载所有加载的Bundle
			BuildLogger.Log("构建结果验证成功！");
		}

		/// <summary>
		/// 解析.manifest文件并获取资源列表
		/// </summary>
		private string[] GetAssetBundleAllAssets(string filePath)
		{
			string manifestFilePath = $"{filePath}.manifest";
			List<string> assetLines = new List<string>();
			using (StreamReader reader = File.OpenText(manifestFilePath))
			{
				string content;
				bool findTarget = false;
				while (null != (content = reader.ReadLine()))
				{
					if (content.StartsWith("Dependencies:"))
						break;
					if (findTarget == false && content.StartsWith("Assets:"))
						findTarget = true;
					if (findTarget)
					{
						if (content.StartsWith("- "))
						{
							string assetPath = content.TrimStart("- ".ToCharArray());
							assetLines.Add(assetPath);
						}
					}
				}
			}
			return assetLines.ToArray();
		}
	}
}