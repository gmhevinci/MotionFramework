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
using MotionFramework.Utility;

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
		/// 制作AssetBundle的完整名称
		/// 注意：名称为全部小写并且包含后缀名
		/// </summary>
		public static string MakeAssetBundleFullName(string bundleLabel, string bundleVariant)
		{
			if (string.IsNullOrEmpty(bundleVariant))
				return bundleLabel.ToLower();
			else
				return $"{bundleLabel}.{bundleVariant}".ToLower();
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
			// 补丁清单路径
			string filePath = $"{outputRoot}/{buildTarget}/{PatchDefine.UnityManifestFileName}/{PatchDefine.PatchManifestFileName}";
			if (File.Exists(filePath) == false)
				throw new System.Exception($"Not found {PatchDefine.PatchManifestFileName} file : {filePath}");

			// 加载补丁清单
			string jsonData = FileUtility.ReadFile(filePath);
			PatchManifest pm = PatchManifest.Deserialize(jsonData);

			// 拷贝文件列表
			foreach(var element in pm.ElementList)
			{
				if (element.IsDLC())
					continue;

				if (targetVersion >= 0 && element.Version > targetVersion)
					continue;

				string sourcePath = $"{outputRoot}/{buildTarget}/{element.Version}/{element.MD5}";
				string destPath = $"{Application.dataPath}/StreamingAssets/{element.MD5}";
				Debug.Log($"拷贝版本文件到流目录：{destPath}");
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝核心文件
			{
				string destFilePath = $"{Application.dataPath}/StreamingAssets/{PatchDefine.PatchManifestFileName}";
				EditorTools.CopyFile(filePath, destFilePath, true);
			}

			// 刷新目录
			AssetDatabase.Refresh();
		}
	}
}