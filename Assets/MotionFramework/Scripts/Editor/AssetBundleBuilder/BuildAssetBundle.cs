//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public static class BuildAssetBundle
	{
		// 程序外部调用接口
		static void BuildAndroid()
		{
			BuildInternal(BuildTarget.Android);
		}
		static void BuildIOS()
		{
			BuildInternal(BuildTarget.iOS);
		}

		// 内部构建方法
		private static void BuildInternal(BuildTarget buildTarget)
		{
			Debug.Log($"[Build] 开始构建补丁包 : {buildTarget}");

			// 打印命令行参数
			int buildVersion = GetBuildVersion();
			bool isForceBuild = IsForceBuild();
			Debug.Log($"[Build] Version : {buildVersion}");
			Debug.Log($"[Build] 强制重建 : {isForceBuild}");

			// 创建AssetBuilder
			AssetBundleBuilder builder = new AssetBundleBuilder(buildTarget, buildVersion);

			// 设置配置
			builder.CompressOption = AssetBundleBuilder.ECompressOption.ChunkBasedCompressionLZ4;
			builder.IsForceRebuild = isForceBuild;
			builder.IsAppendHash = false;
			builder.IsDisableWriteTypeTree = false;
			builder.IsIgnoreTypeTreeChanges = true;

			// 执行构建
			builder.PreAssetBuild();
			builder.PostAssetBuild();

			// 构建成功
			Debug.Log("[Build] 构建完成");
		}

		// 从构建命令里获取参数
		private static int GetBuildVersion()
		{
			foreach (string arg in System.Environment.GetCommandLineArgs())
			{
				if (arg.StartsWith("buildVersion"))
					return int.Parse(arg.Split("="[0])[1]);
			}
			return -1;
		}
		private static bool IsForceBuild()
		{
			foreach (string arg in System.Environment.GetCommandLineArgs())
			{
				if (arg.StartsWith("forceBuild"))
					return arg.Split("="[0])[1] == "true" ? true : false;
			}
			return false;
		}
	}
}