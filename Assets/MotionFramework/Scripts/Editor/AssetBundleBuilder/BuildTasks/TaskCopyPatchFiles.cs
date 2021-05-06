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
	/// 拷贝补丁文件
	/// </summary>
	internal class TaskCopyPatchFiles : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			CopyPatchFiles(buildParameters);
		}

		/// <summary>
		/// 拷贝补丁文件到补丁包目录
		/// </summary>
		private void CopyPatchFiles(AssetBundleBuilder.BuildParametersContext buildParameters)
		{
			string packageDirectory = buildParameters.GetPackageDirectory();
			BuildLogger.Log($"开始拷贝补丁文件到补丁包目录：{packageDirectory}");

			// 拷贝Readme文件
			{
				string sourcePath = $"{buildParameters.OutputDirectory}/{PatchDefine.ReadmeFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.ReadmeFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				BuildLogger.Log($"拷贝Readme文件到：{destPath}");
			}

			// 拷贝PatchManifest文件
			{
				string sourcePath = $"{buildParameters.OutputDirectory}/{PatchDefine.PatchManifestFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.PatchManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				BuildLogger.Log($"拷贝PatchManifest文件到：{destPath}");
			}

			// 拷贝UnityManifest序列化文件
			{
				string sourcePath = $"{buildParameters.OutputDirectory}/{PatchDefine.UnityManifestFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.UnityManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				BuildLogger.Log($"拷贝UnityManifest文件到：{destPath}");
			}

			// 拷贝UnityManifest文本文件
			{
				string sourcePath = $"{buildParameters.OutputDirectory}/{PatchDefine.UnityManifestFileName}.manifest";
				string destPath = $"{packageDirectory}/{PatchDefine.UnityManifestFileName}.manifest";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝所有补丁文件
			// 注意：拷贝的补丁文件都是需要玩家热更新的文件
			int progressValue = 0;
			PatchManifest patchFile = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.OutputDirectory);
			int patchFileTotalCount = patchFile.BundleList.Count;
			foreach (var patchBundle in patchFile.BundleList)
			{
				if (patchBundle.Version == buildParameters.Parameters.BuildVersion)
				{
					string sourcePath = $"{buildParameters.OutputDirectory}/{patchBundle.BundleName}";
					string destPath = $"{packageDirectory}/{patchBundle.Hash}";
					EditorTools.CopyFile(sourcePath, destPath, true);
					BuildLogger.Log($"拷贝补丁文件到补丁包：{sourcePath}");
					EditorTools.DisplayProgressBar("拷贝补丁文件", ++progressValue, patchFileTotalCount);
				}
			}
			EditorTools.ClearProgressBar();
		}
	}
}