//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MotionFramework.Patch;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 复制更新文件
	/// </summary>
	internal class TaskCopyUpdateFiles : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			CopyUpdateFiles(buildParameters);
		}

		/// <summary>
		/// 复制更新文件到补丁包目录
		/// </summary>
		private void CopyUpdateFiles(AssetBundleBuilder.BuildParametersContext buildParameters)
		{
			string packageDirectory = buildParameters.GetPackageDirectory();
			BuildLogger.Log($"开始复制更新文件到补丁包目录：{packageDirectory}");

			// 复制Readme文件
			{
				string sourcePath = $"{buildParameters.OutputDirectory}/{PatchDefine.ReadmeFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.ReadmeFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				BuildLogger.Log($"复制Readme文件到：{destPath}");
			}

			// 复制PatchManifest文件
			{
				string sourcePath = $"{buildParameters.OutputDirectory}/{PatchDefine.PatchManifestFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.PatchManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				BuildLogger.Log($"复制PatchManifest文件到：{destPath}");
			}

			// 复制UnityManifest文件
			{
				string sourcePath = $"{buildParameters.OutputDirectory}/{PatchDefine.UnityManifestFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.UnityManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				BuildLogger.Log($"复制UnityManifest文件到：{destPath}");
			}

			// 复制Manifest文件
			{
				string sourcePath = $"{buildParameters.OutputDirectory}/{PatchDefine.UnityManifestFileName}.manifest";
				string destPath = $"{packageDirectory}/{PatchDefine.UnityManifestFileName}.manifest";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 复制所有更新文件
			int progressBarCount = 0;
			PatchManifest patchFile = AssetBundleBuilder.LoadPatchManifestFile(buildParameters.OutputDirectory);
			int patchFileTotalCount = patchFile.BundleList.Count;
			foreach (var patchBundle in patchFile.BundleList)
			{
				if (patchBundle.Version == buildParameters.Parameters.BuildVersion)
				{
					string sourcePath = $"{buildParameters.OutputDirectory}/{patchBundle.BundleName}";
					string destPath = $"{packageDirectory}/{patchBundle.Hash}";
					EditorTools.CopyFile(sourcePath, destPath, true);
					BuildLogger.Log($"复制更新文件到补丁包：{sourcePath}");

					progressBarCount++;
					EditorUtility.DisplayProgressBar("进度", $"拷贝更新文件 : {sourcePath}", (float)progressBarCount / patchFileTotalCount);
				}
			}
			EditorUtility.ClearProgressBar();
		}
	}
}