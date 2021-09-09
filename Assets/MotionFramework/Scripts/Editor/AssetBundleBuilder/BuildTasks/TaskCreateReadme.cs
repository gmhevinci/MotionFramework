//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MotionFramework.Patch;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 创建说明文件
	/// </summary>
	public class TaskCreateReadme : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildMapContext = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();
			CreateReadmeFile(buildParameters, buildMapContext);
		}

		/// <summary>
		/// 创建Readme文件到输出目录
		/// </summary>
		private void CreateReadmeFile(AssetBundleBuilder.BuildParametersContext buildParameters, TaskGetBuildMap.BuildMapContext buildMapContext)
		{
			// 删除旧文件
			string filePath = $"{buildParameters.PipelineOutputDirectory}/{PatchDefine.ReadmeFileName}";
			if (File.Exists(filePath))
				File.Delete(filePath);

			BuildLogger.Log($"创建说明文件：{filePath}");

			StringBuilder content = new StringBuilder();
			AppendData(content, $"构建平台：{buildParameters.Parameters.BuildTarget}");
			AppendData(content, $"构建版本：{buildParameters.Parameters.BuildVersion}");
			AppendData(content, $"构建时间：{DateTime.Now}");

			AppendData(content, "");
			AppendData(content, $"--着色器--");
			AppendData(content, $"IsCollectAllShaders：{AssetBundleCollectorSettingData.Setting.IsCollectAllShaders}");
			AppendData(content, $"ShadersBundleName：{AssetBundleCollectorSettingData.Setting.ShadersBundleName}");

			AppendData(content, "");
			AppendData(content, $"--配置信息--");
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = AssetBundleCollectorSettingData.Setting.Collectors[i];
				AppendData(content, wrapper.ToString());
			}

			AppendData(content, "");
			AppendData(content, $"--构建参数--");
			AppendData(content, $"CompressOption：{buildParameters.Parameters.CompressOption}");
			AppendData(content, $"IsForceRebuild：{buildParameters.Parameters.IsForceRebuild}");
			AppendData(content, $"BuildinTags：{buildParameters.Parameters.BuildinTags}");
			AppendData(content, $"IsAppendHash：{buildParameters.Parameters.IsAppendHash}");
			AppendData(content, $"IsDisableWriteTypeTree：{buildParameters.Parameters.IsDisableWriteTypeTree}");
			AppendData(content, $"IsIgnoreTypeTreeChanges：{buildParameters.Parameters.IsIgnoreTypeTreeChanges}");

			AppendData(content, "");
			AppendData(content, $"--构建信息--");
			AppendData(content, $"参与构建的资源总数：{buildMapContext.GetAllAssets().Count}");
			AppendData(content, $"构建的AB资源包总数：{buildMapContext.GetBuildAssetBundleCount()}");
			AppendData(content, $"构建的原生资源包总数：{buildMapContext.GetBuildRawBundleCount()}");

			AppendData(content, "");
			AppendData(content, $"--构建列表--");
			for (int i = 0; i < buildMapContext.BundleInfos.Count; i++)
			{
				string bundleName = buildMapContext.BundleInfos[i].BundleName;
				AppendData(content, bundleName);
			}

			PatchManifest patchManifest = AssetBundleBuilderHelper.LoadPatchManifestFile(buildParameters.PipelineOutputDirectory);
			{
				AppendData(content, "");
				AppendData(content, $"--内置列表--");
				foreach (var patchBundle in patchManifest.BundleList)
				{
					if (patchBundle.IsBuildin)
					{
						AppendData(content, patchBundle.BundleName);
					}
				}

				AppendData(content, "");
				AppendData(content, $"--更新列表--");
				foreach (var patchBundle in patchManifest.BundleList)
				{
					if (patchBundle.Version == buildParameters.Parameters.BuildVersion)
					{
						AppendData(content, patchBundle.BundleName);
					}
				}

				AppendData(content, "");
				AppendData(content, $"--加密列表--");
				foreach (var patchBundle in patchManifest.BundleList)
				{
					if (patchBundle.IsEncrypted)
					{
						AppendData(content, patchBundle.BundleName);
					}
				}
			}

			// 创建新文件
			File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
		}
		private void AppendData(StringBuilder sb, string data)
		{
			sb.Append(data);
			sb.Append("\r\n");
		}
	}
}