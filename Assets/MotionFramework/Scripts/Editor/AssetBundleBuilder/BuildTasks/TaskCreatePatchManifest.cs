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
			CreatePatchManifestFile(buildParameters, buildMapContext, unityManifestContext.Manifest, encryptionContext.EncryptList);
		}

		/// <summary>
		/// 创建补丁清单文件到输出目录
		/// </summary>
		private void CreatePatchManifestFile(AssetBundleBuilder.BuildParametersContext buildParameters, TaskGetBuildMap.BuildMapContext buildMapContext, AssetBundleManifest unityManifest, List<string> encryptList)
		{
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();

			// 创建DLC管理器
			DLCManager dlcManager = new DLCManager();
			dlcManager.LoadAllDLC();

			// 加载旧补丁清单
			PatchManifest oldPatchManifest = AssetBundleBuilder.LoadPatchManifestFile(buildParameters);

			// 创建新补丁清单
			PatchManifest newPatchManifest = new PatchManifest();

			// 写入版本信息
			newPatchManifest.ResourceVersion = buildParameters.BuildVersion;

			// 写入所有AssetBundle文件的信息
			for (int i = 0; i < allAssetBundles.Length; i++)
			{
				string bundleName = allAssetBundles[i];
				string path = $"{buildParameters.OutputDirectory}/{bundleName}";
				string md5 = HashUtility.FileMD5(path);
				uint crc32 = HashUtility.FileCRC32(path);
				long sizeBytes = EditorTools.GetFileSize(path);
				int version = buildParameters.BuildVersion;
				string[] assetPaths = buildMapContext.GetAssetPaths(bundleName);
				string[] depends = unityManifest.GetDirectDependencies(bundleName);
				string[] dlcLabels = dlcManager.GetAssetBundleDLCLabels(bundleName);

				// 创建标记位
				bool isEncrypted = encryptList.Contains(bundleName);
				bool isCollected = buildMapContext.IsCollectBundle(bundleName);
				int flags = PatchElement.CreateFlags(isEncrypted, isCollected);

				// 注意：如果文件没有变化使用旧版本号
				if (oldPatchManifest.Elements.TryGetValue(bundleName, out PatchElement oldElement))
				{
					if (oldElement.MD5 == md5)
						version = oldElement.Version;
				}

				PatchElement newElement = new PatchElement(bundleName, md5, crc32, sizeBytes, version, flags, assetPaths, depends, dlcLabels);
				newPatchManifest.ElementList.Add(newElement);
			}

			// 写入所有变体信息
			{
				Dictionary<string, List<string>> variantInfos = GetVariantInfos(allAssetBundles);
				foreach (var pair in variantInfos)
				{
					if (pair.Value.Count > 0)
					{
						string bundleName = $"{pair.Key}.{ PatchDefine.AssetBundleDefaultVariant}";
						List<string> variants = pair.Value;
						newPatchManifest.VariantList.Add(new PatchVariant(bundleName, variants));
					}
				}
			}

			// 创建新文件
			string filePath = buildParameters.OutputDirectory + $"/{PatchDefine.PatchManifestFileName}";
			BuildLogger.Log($"创建补丁清单文件：{filePath}");
			PatchManifest.Serialize(filePath, newPatchManifest);
		}
		
		/// <summary>
		/// 获取所有变种信息
		/// </summary>
		private Dictionary<string, List<string>> GetVariantInfos(string[] allAssetBundles)
		{
			Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
			foreach (var assetBundleLabel in allAssetBundles)
			{
				if (Path.HasExtension(assetBundleLabel) == false)
					continue;

				string path = assetBundleLabel.RemoveExtension();
				string extension = Path.GetExtension(assetBundleLabel).Substring(1);

				if (dic.ContainsKey(path) == false)
					dic.Add(path, new List<string>());

				if (extension != PatchDefine.AssetBundleDefaultVariant)
					dic[path].Add(extension);
			}
			return dic;
		}
	}
}