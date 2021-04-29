//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public static class AssetBundleBuilderTools
	{
		/// <summary>
		/// 检测预制件是否损坏
		/// </summary>
		public static void CheckAllPrefabValid()
		{
			// 获取所有的打包路径
			List<string> collectDirectorys = AssetBundleCollectorSettingData.GetAllCollectDirectory();
			if (collectDirectorys.Count == 0)
				throw new Exception("[BuildPackage] 打包路径列表不能为空");

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
					Debug.LogError($"[Build] 发现损坏预制件：{assetPath}");
				}

				// 进度条相关
				checkCount++;
				EditorUtility.DisplayProgressBar("进度", $"检测预制件文件是否损坏：{checkCount}/{guids.Length}", (float)checkCount / guids.Length);
			}

			EditorUtility.ClearProgressBar();
			if (invalidCount == 0)
				Debug.Log($"没有发现损坏预制件");
		}

		/// <summary>
		/// 清理无用的材质球属性
		/// </summary>
		public static void ClearMaterialUnusedProperty()
		{
			// 获取所有的打包路径
			List<string> collectDirectorys = AssetBundleCollectorSettingData.GetAllCollectDirectory();
			if (collectDirectorys.Count == 0)
				throw new Exception("[BuildPackage] 打包路径列表不能为空");

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
					Debug.LogWarning($"[Build] 材质球已被处理：{assetPath}");
				}

				// 进度条相关
				checkCount++;
				EditorUtility.DisplayProgressBar("进度", $"清理无用的材质球属性：{checkCount}/{guids.Length}", (float)checkCount / guids.Length);
			}

			EditorUtility.ClearProgressBar();
			if (removedCount == 0)
				Debug.Log($"没有发现冗余的材质球属性");
			else
				AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// 刷新流目录
		/// </summary>
		public static void RefreshStreammingFolder(BuildTarget buildTarget)
		{
			string streamingDirectory = Application.dataPath + "/StreamingAssets";
			EditorTools.ClearFolder(streamingDirectory);

			string outputRoot = AssetBundleBuilderHelper.GetDefaultOutputRootPath();
			AssetBundleBuilderHelper.CopyPackageToStreamingFolder(buildTarget, outputRoot);
		}
	}
}