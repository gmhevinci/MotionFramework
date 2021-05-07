//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using MotionFramework.Patch;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	public class AssetBundleBuilder
	{
		/// <summary>
		/// 构建参数
		/// </summary>
		public class BuildParameters
		{
			/// <summary>
			/// 输出的根目录
			/// </summary>
			public string OutputRoot;

			/// <summary>
			/// 构建的平台
			/// </summary>
			public BuildTarget BuildTarget;

			/// <summary>
			/// 构建的版本（资源版本号）
			/// </summary>
			public int BuildVersion;

			/// <summary>
			/// 压缩选项
			/// </summary>
			public ECompressOption CompressOption;

			/// <summary>
			/// 是否强制重新构建整个项目，如果为FALSE则是增量打包
			/// </summary>
			public bool IsForceRebuild;

			#region 高级选项
			/// <summary>
			/// 文件名附加上哈希值
			/// </summary>
			public bool IsAppendHash = false;

			/// <summary>
			/// 禁止写入类型树结构
			/// </summary>
			public bool IsDisableWriteTypeTree = false;

			/// <summary>
			/// 忽略类型树变化
			/// </summary>
			public bool IsIgnoreTypeTreeChanges = true;
			#endregion
		}

		/// <summary>
		/// 构建参数环境
		/// </summary>
		public class BuildParametersContext : IContextObject
		{
			public BuildParameters Parameters { private set; get; }

			/// <summary>
			/// 最终的输出目录
			/// </summary>
			public string OutputDirectory { private set; get; }

			public BuildParametersContext(BuildParameters parameters)
			{
				Parameters = parameters;
				OutputDirectory = AssetBundleBuilderHelper.MakeOutputDirectory(parameters.OutputRoot, parameters.BuildTarget);
			}

			/// <summary>
			/// 获取本次构建的补丁目录
			/// </summary>
			public string GetPackageDirectory()
			{
				return $"{Parameters.OutputRoot}/{Parameters.BuildTarget}/{Parameters.BuildVersion}";
			}

			/// <summary>
			/// 获取构建选项
			/// </summary>
			public BuildAssetBundleOptions GetPiplineBuildOptions()
			{
				// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
				// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

				BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
				opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

				if (Parameters.CompressOption == ECompressOption.Uncompressed)
					opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
				else if (Parameters.CompressOption == ECompressOption.LZ4)
					opt |= BuildAssetBundleOptions.ChunkBasedCompression;

				if (Parameters.IsForceRebuild)
					opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
				if (Parameters.IsAppendHash)
					opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName; //Append the hash to the assetBundle name
				if (Parameters.IsDisableWriteTypeTree)
					opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
				if (Parameters.IsIgnoreTypeTreeChanges)
					opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

				return opt;
			}
		}


		private readonly BuildContext _buildContext = new BuildContext();

		/// <summary>
		/// 开始构建
		/// </summary>
		public bool Run(BuildParameters buildParameters)
		{
			// 清空旧数据
			_buildContext.ClearAllContext();

			// 构建参数
			var buildParametersContext = new BuildParametersContext(buildParameters);
			_buildContext.SetContextObject(buildParametersContext);

			// 执行构建流程
			List<IBuildTask> pipeline = new List<IBuildTask>
			{
				new TaskPrepare(), //前期准备工作
				new TaskGetBuildMap(), //获取构建列表
				new TaskBuilding(), //开始执行构建
				new TaskEncryption(), //加密资源文件
				new TaskCheckCycle(), //检测循环依赖
				new TaskCreatePatchManifest(), //创建清单文件
				new TaskCreateReadme(), //创建说明文件
				new TaskCopyPatchFiles() //拷贝补丁文件
			};

			bool succeed = BuildRunner.Run(pipeline, _buildContext);
			if(succeed)
				BuildLogger.Log($"构建成功！");
			else
				BuildLogger.Warning($"构建失败！");
			return succeed;
		}
	}
}