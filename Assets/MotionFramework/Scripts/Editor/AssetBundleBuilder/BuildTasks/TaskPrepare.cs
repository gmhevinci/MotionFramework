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
using MotionFramework.Patch;

namespace MotionFramework.Editor
{
	internal class TaskPrepare : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();		

			// 检测构建平台是否合法
			if (buildParameters.Parameters.BuildTarget == BuildTarget.NoTarget)
				throw new Exception("请选择目标平台");

			// 检测构建版本是否合法
			if (EditorTools.IsNumber(buildParameters.Parameters.BuildVersion.ToString()) == false)
				throw new Exception($"版本号格式非法：{buildParameters.Parameters.BuildVersion}");
			if (buildParameters.Parameters.BuildVersion < 0)
				throw new Exception("请先设置版本号");

			// 检测输出目录是否为空
			if (string.IsNullOrEmpty(buildParameters.PipelineOutputDirectory))
				throw new Exception("输出目录不能为空");

			// 检测补丁包是否已经存在
			string packageDirectory = buildParameters.GetPackageDirectory();
			if (Directory.Exists(packageDirectory))
				throw new Exception($"补丁包已经存在：{packageDirectory}");

			// 检测资源收集配置文件
			if (AssetBundleCollectorSettingData.GetCollecterCount() == 0)
				throw new Exception("配置的资源收集路径为空！");

			// 检测内置资源标记是否一致
			if (buildParameters.Parameters.IsForceRebuild == false)
			{
				PatchManifest oldPatchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory);
				if (buildParameters.Parameters.BuildinTags != oldPatchManifest.BuildinTags)
					throw new Exception($"增量更新时内置资源标记必须一致：{buildParameters.Parameters.BuildinTags} != {oldPatchManifest.BuildinTags}");
			}

			// 如果是强制重建
			if (buildParameters.Parameters.IsForceRebuild)
			{
				// 删除平台总目录
				string platformDirectory = $"{buildParameters.Parameters.OutputRoot}/{buildParameters.Parameters.BuildTarget}";
				if (EditorTools.DeleteDirectory(platformDirectory))
				{
					BuildLogger.Log($"删除平台总目录：{platformDirectory}");
				}
			}

			// 如果输出目录不存在
			if (EditorTools.CreateDirectory(buildParameters.PipelineOutputDirectory))
			{
				BuildLogger.Log($"创建输出目录：{buildParameters.PipelineOutputDirectory}");
			}
		}
	}
}