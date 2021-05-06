//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public static class AssetBundleBuilderTools
	{
		/// <summary>
		/// 检测所有损坏的预制体文件
		/// </summary>
		public static void CheckCorruptionPrefab()
		{
			// 获取所有的打包路径
			List<string> collectDirectorys = AssetBundleCollectorSettingData.GetAllCollectDirectory();
			if (collectDirectorys.Count == 0)
				throw new Exception("打包路径列表不能为空");

			// 获取所有资源列表
			int checkCount = 0;
			int invalidCount = 0;
			string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Prefab}", collectDirectorys.ToArray());
			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
				if (prefab == null)
				{
					invalidCount++;
					Debug.LogError($"发现损坏预制件：{assetPath}");
				}
				EditorTools.DisplayProgressBar("检测预制件文件是否损坏", ++checkCount, guids.Length);
			}
			EditorTools.ClearProgressBar();

			if (invalidCount == 0)
				Debug.Log($"没有发现损坏预制件");
		}

		/// <summary>
		/// 检测所有重名的着色器文件
		/// </summary>
		public static void CheckSameNameShader()
		{
			Dictionary<string, string> temper = new Dictionary<string, string>();

			// 获取所有的打包路径
			List<string> collectDirectorys = AssetBundleCollectorSettingData.GetAllCollectDirectory();
			if (collectDirectorys.Count == 0)
				throw new Exception("打包路径列表不能为空");

			// 获取所有资源列表
			int checkCount = 0;
			int invalidCount = 0;
			string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Shader}", collectDirectorys.ToArray());
			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				string fileName = Path.GetFileName(assetPath);
				if (temper.ContainsKey(fileName) == false)
				{
					temper.Add(fileName, assetPath);
				}
				else
				{
					Debug.LogWarning($"发现重名着色器：{assetPath} {temper[fileName]}");
				}
				EditorTools.DisplayProgressBar("检测着色器文件是否重名", ++checkCount, guids.Length);
			}
			EditorTools.ClearProgressBar();

			if (invalidCount == 0)
				Debug.Log($"没有发现重名着色器");
		}

		/// <summary>
		/// 清理无用的材质球属性
		/// </summary>
		public static void ClearMaterialUnusedProperty()
		{
			// 获取所有的打包路径
			List<string> collectDirectorys = AssetBundleCollectorSettingData.GetAllCollectDirectory();
			if (collectDirectorys.Count == 0)
				throw new Exception("打包路径列表不能为空");

			// 获取所有资源列表
			int checkCount = 0;
			int removedCount = 0;
			string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Material}", collectDirectorys.ToArray());
			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
				bool removed = EditorTools.ClearMaterialUnusedProperty(mat);
				if (removed)
				{
					removedCount++;
					Debug.LogWarning($"材质球已被处理：{assetPath}");
				}
				EditorTools.DisplayProgressBar("清理无用的材质球属性", ++checkCount, guids.Length);
			}
			EditorTools.ClearProgressBar();

			if (removedCount == 0)
				Debug.Log($"没有发现冗余的材质球属性");
			else
				AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// 拷贝补丁文件到流目录
		/// </summary>
		public static void CopyPatchFilesToStreamming(bool clearStreamming, BuildTarget buildTarget)
		{
			if (clearStreamming)
			{
				AssetBundleBuilderHelper.ClearStreamingAssetsFolder();
			}

			string outputRoot = AssetBundleBuilderHelper.GetDefaultOutputRootPath();
			AssetBundleBuilderHelper.CopyPackageToStreamingFolder(buildTarget, outputRoot);
		}
	}
}