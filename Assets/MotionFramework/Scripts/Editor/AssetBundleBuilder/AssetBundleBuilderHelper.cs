//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using MotionFramework.Patch;

namespace MotionFramework.Editor
{
	public static class AssetBundleBuilderHelper
	{
		/// <summary>
		/// 获取默认的导出根路径
		/// </summary>
		public static string GetDefaultOutputRootPath()
		{
			string projectPath = EditorTools.GetProjectPath();
			return $"{projectPath}/BuildBundles";
		}


		/// <summary>
		/// 清空流文件夹
		/// </summary>
		public static void ClearStreamingAssetsFolder()
		{
			string streamingPath = Application.dataPath + "/StreamingAssets";
			EditorTools.ClearFolder(streamingPath);
		}

		/// <summary>
		/// 删除流文件夹内无关的文件
		/// 删除.manifest文件和.meta文件
		/// </summary>
		public static void DeleteStreamingAssetsIgnoreFiles()
		{
			string streamingPath = Application.dataPath + "/StreamingAssets";
			if (Directory.Exists(streamingPath))
			{
				string[] files = Directory.GetFiles(streamingPath, "*.manifest", SearchOption.AllDirectories);
				foreach (var file in files)
				{
					FileInfo info = new FileInfo(file);
					info.Delete();
				}

				files = Directory.GetFiles(streamingPath, "*.meta", SearchOption.AllDirectories);
				foreach (var item in files)
				{
					FileInfo info = new FileInfo(item);
					info.Delete();
				}
			}
		}


		/// <summary>
		/// 获取所有补丁包版本列表
		/// 注意：列表会按照版本号从小到大排序
		/// </summary>
		private static List<int> GetPackageVersionList(BuildTarget buildTarget, string outputRoot)
		{
			// 获取所有补丁包文件夹
			string parentPath = $"{outputRoot}/{buildTarget}";
			string[] allFolders = Directory.GetDirectories(parentPath);
			List<int> versionList = new List<int>();
			for (int i = 0; i < allFolders.Length; i++)
			{
				string folderName = Path.GetFileNameWithoutExtension(allFolders[i]);
				int version;
				if (int.TryParse(folderName, out version))
					versionList.Add(version);
			}

			// 从小到大排序
			versionList.Sort();
			return versionList;
		}

		/// <summary>
		/// 获取当前最大的补丁包版本号
		/// </summary>
		/// <returns>如果没有任何补丁版本，那么返回-1</returns>
		public static int GetMaxPackageVersion(BuildTarget buildTarget, string outputRoot)
		{
			List<int> versionList = GetPackageVersionList(buildTarget, outputRoot);
			if (versionList.Count == 0)
				return -1;
			return versionList[versionList.Count - 1];
		}

		/// <summary>
		/// 复制所有补丁包文件到流目录
		/// </summary>
		/// <param name="targetVersion">目标版本。如果版本为负值则拷贝所有版本</param>
		public static void CopyPackageToStreamingFolder(BuildTarget buildTarget, string outputRoot, int targetVersion = -1)
		{
			string parentPath = $"{outputRoot}/{buildTarget}";
			string streamingPath = Application.dataPath + "/StreamingAssets";

			// 获取所有补丁包版本列表
			List<int> versionList = GetPackageVersionList(buildTarget, outputRoot);

			// 拷贝资源
			for (int i = 0; i < versionList.Count; i++)
			{
				if (targetVersion >= 0 && versionList[i] > targetVersion)
					break;

				string sourcePath = $"{parentPath}/{versionList[i]}";
				Debug.Log($"拷贝版本文件到流目录：{sourcePath}");
				EditorTools.CopyDirectory(sourcePath, streamingPath);
			}

			// 刷新目录
			AssetDatabase.Refresh();
		}

		/// <summary>
		/// 复制补丁文件到输出目录
		/// <param name="targetVersion">目标版本。如果版本为负值则拷贝所有版本</param>
		/// </summary>
		public static void CopyPackageToUnityManifestFolder(BuildTarget buildTarget, string outputRoot, int targetVersion = -1)
		{
			string parentPath = $"{outputRoot}/{buildTarget}";
			string outputPath = $"{outputRoot}/{buildTarget}/{PatchDefine.UnityManifestFileName}";

			// 获取所有补丁包版本列表
			List<int> versionList = GetPackageVersionList(buildTarget, outputRoot);

			// 拷贝资源
			for (int i = 0; i < versionList.Count; i++)
			{
				if (targetVersion >= 0 && versionList[i] > targetVersion)
					break;

				string sourcePath = $"{parentPath}/{versionList[i]}";
				Debug.Log($"拷贝版本文件到主目录：{sourcePath}");
				EditorTools.CopyDirectory(sourcePath, outputPath);
			}
		}
	}
}