//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MotionFramework.Patch;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 创建补丁清单文件
	/// </summary>
	internal class TaskCreatePatchManifest : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var unityManifestContext = context.GetContextObject<TaskBuilding.UnityManifestContext>();
			var encryptionContext = context.GetContextObject<TaskEncryption.EncryptionContext>();
			var buildMapContext = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();
			CreatePatchManifestFile(buildParameters, buildMapContext, encryptionContext, unityManifestContext.Manifest);
		}

		/// <summary>
		/// 创建补丁清单文件到输出目录
		/// </summary>
		private void CreatePatchManifestFile(AssetBundleBuilder.BuildParametersContext buildParameters,
			TaskGetBuildMap.BuildMapContext buildMapContext, TaskEncryption.EncryptionContext encryptionContext,
			AssetBundleManifest unityManifest)
		{
			// 创建新补丁清单
			PatchManifest patchManifest = new PatchManifest();
			patchManifest.ResourceVersion = buildParameters.Parameters.BuildVersion;
			patchManifest.BundleList = GetAllPatchBundle(buildParameters, buildMapContext, encryptionContext, unityManifest);
			patchManifest.VariantList = GetAllPatchVariant(unityManifest);

			// 创建新文件
			string filePath = $"{buildParameters.OutputDirectory}/{PatchDefine.PatchManifestFileName}";
			BuildLogger.Log($"创建补丁清单文件：{filePath}");
			PatchManifest.Serialize(filePath, patchManifest);
		}

		/// <summary>
		/// 获取资源包列表
		/// </summary>
		private List<PatchBundle> GetAllPatchBundle(AssetBundleBuilder.BuildParametersContext buildParameters,
			TaskGetBuildMap.BuildMapContext buildMapContext, TaskEncryption.EncryptionContext encryptionContext,
			AssetBundleManifest unityManifest)
		{
			List<PatchBundle> result = new List<PatchBundle>();

			// 加载DLC
			DLCManager dlcManager = new DLCManager();
			dlcManager.LoadAllDLC();

			// 加载旧补丁清单
			PatchManifest oldPatchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.OutputDirectory);

			// 获取加密列表
			List<string> encryptList = encryptionContext.EncryptList;

			string[] allAssetBundles = unityManifest.GetAllAssetBundles();
			foreach (var bundleName in allAssetBundles)
			{
				string path = $"{buildParameters.OutputDirectory}/{bundleName}";
				string hash = HashUtility.FileMD5(path);
				string crc = HashUtility.FileCRC32(path);
				long size = FileUtility.GetFileSize(path);
				int version = buildParameters.Parameters.BuildVersion;
				string[] assets = buildMapContext.GetCollectAssetPaths(bundleName);
				string[] depends = unityManifest.GetDirectDependencies(bundleName);
				string[] dlcLabels = dlcManager.GetAssetBundleDLCLabels(bundleName);

				// 创建标记位
				bool isEncrypted = encryptList.Contains(bundleName);
				int flags = PatchBundle.CreateFlags(isEncrypted);

				// 注意：如果文件没有变化使用旧版本号
				if (oldPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle oldElement))
				{
					if (oldElement.Hash == hash)
						version = oldElement.Version;
				}

				PatchBundle newElement = new PatchBundle(bundleName, hash, crc, size, version, flags, assets, depends, dlcLabels);
				result.Add(newElement);
			}

			return result;
		}

		/// <summary>
		/// 获取变种列表
		/// </summary>
		private List<PatchVariant> GetAllPatchVariant(AssetBundleManifest unityManifest)
		{
			Dictionary<string, List<string>> variantInfos = new Dictionary<string, List<string>>();
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();
			foreach (var bundleName in allAssetBundles)
			{
				if (Path.HasExtension(bundleName) == false)
					continue;

				string path = bundleName.RemoveExtension();
				string extension = Path.GetExtension(bundleName).Substring(1);

				if (variantInfos.ContainsKey(path) == false)
					variantInfos.Add(path, new List<string>());

				if (extension != PatchDefine.AssetBundleDefaultVariant)
					variantInfos[path].Add(extension);
			}

			List<PatchVariant> result = new List<PatchVariant>();
			foreach (var pair in variantInfos)
			{
				if (pair.Value.Count > 0)
				{
					string bundleName = $"{pair.Key}.{ PatchDefine.AssetBundleDefaultVariant}";
					List<string> variants = pair.Value;
					result.Add(new PatchVariant(bundleName, variants));
				}
			}
			return result;
		}
	}
}