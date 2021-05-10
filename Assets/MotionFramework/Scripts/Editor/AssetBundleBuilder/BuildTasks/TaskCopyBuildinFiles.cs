//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using MotionFramework.Patch;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 拷贝内置文件到StreamingAssets
	/// </summary>
	public class TaskCopyBuildinFiles : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			// 注意：我们只有在强制重建的时候才会拷贝
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			if(buildParameters.Parameters.IsForceRebuild)
			{
				// 清空流目录
				AssetBundleBuilderHelper.ClearStreamingAssetsFolder();

				// 拷贝内置文件
				var pipelineOutputDirectory = buildParameters.PipelineOutputDirectory;
				CopyBuildinFilesToStreaming(pipelineOutputDirectory);
			}
		}

		private void CopyBuildinFilesToStreaming(string pipelineOutputDirectory)
		{
			// 加载补丁清单
			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(pipelineOutputDirectory);

			// 拷贝文件列表
			foreach (var patchBundle in patchManifest.BundleList)
			{
				if (patchBundle.IsBuildin == false)
					continue;

				string sourcePath = $"{pipelineOutputDirectory}/{patchBundle.BundleName}";
				string destPath = $"{Application.dataPath}/StreamingAssets/{patchBundle.Hash}";
				BuildLogger.Log($"拷贝内置文件到流目录：{patchBundle.BundleName}");
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 拷贝核心文件
			{
				string sourcePath = $"{pipelineOutputDirectory}/{PatchDefine.PatchManifestFileName}";
				string destPath = $"{Application.dataPath}/StreamingAssets/{PatchDefine.PatchManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 刷新目录
			AssetDatabase.Refresh();
		}
	}
}