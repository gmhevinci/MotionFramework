//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MotionFramework.Patch;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	public class AssetBundleBuilder
	{
		/// <summary>
		/// 构建选项
		/// </summary>
		public class BuildOptionsContext : IContextObject
		{
			public ECompressOption CompressOption = ECompressOption.Uncompressed;
			public bool IsForceRebuild = false;
			public bool IsAppendHash = false;
			public bool IsDisableWriteTypeTree = false;
			public bool IsIgnoreTypeTreeChanges = false;
		}

		/// <summary>
		/// 构建参数
		/// </summary>
		public class BuildParametersContext : IContextObject
		{
			/// <summary>
			/// 输出的根目录
			/// </summary>
			public string OutputRoot { private set; get; }

			/// <summary>
			/// 最终的输出目录
			/// </summary>
			public string OutputDirectory { private set; get; }

			/// <summary>
			/// 构建的平台
			/// </summary>
			public BuildTarget BuildTarget { private set; get; } 

			/// <summary>
			/// 构建的资源版本号
			/// </summary>
			public int BuildVersion { private set; get; }

			public BuildParametersContext(string outputRoot, BuildTarget buildTarget, int buildVersion)
			{
				OutputRoot = outputRoot;
				BuildTarget = buildTarget;
				BuildVersion = buildVersion;
				OutputDirectory = MakeOutputDirectory(outputRoot, buildTarget);
			}

			/// <summary>
			/// 获取本次构建的补丁目录
			/// </summary>
			public string GetPackageDirectory()
			{
				return $"{OutputRoot}/{BuildTarget}/{BuildVersion}";
			}
		}

		private readonly BuildContext _buildContext = new BuildContext();


		/// <summary>
		/// 设置打包参数
		/// </summary>
		/// <param name="buildTarget">构建平台</param>
		/// <param name="buildVersion">构建版本</param>
		public void SetBuildParameters(string outputRoot, BuildTarget buildTarget, int buildVersion)
		{
			BuildParametersContext buildParametersContext = new BuildParametersContext(outputRoot, buildTarget, buildVersion);
			_buildContext.SetContextObject(buildParametersContext);
		}

		/// <summary>
		/// 设置打包选项
		/// </summary>
		/// <param name="compressOption">压缩选项</param>
		/// <param name="isForceRebuild">是否强制重新构建整个项目，如果为FALSE则是增量打包</param>
		public void SetBuildOptions(ECompressOption compressOption, bool isForceRebuild, bool isAppendHash, 
			bool isDisableWriteTypeTree, bool isIgnoreTypeTreeChanges)
		{
			BuildOptionsContext buildOptionsContext = new BuildOptionsContext();
			buildOptionsContext.CompressOption = compressOption;
			buildOptionsContext.IsForceRebuild = isForceRebuild;
			buildOptionsContext.IsAppendHash = isAppendHash;
			buildOptionsContext.IsDisableWriteTypeTree = isDisableWriteTypeTree;
			buildOptionsContext.IsIgnoreTypeTreeChanges = isIgnoreTypeTreeChanges;
			_buildContext.SetContextObject(buildOptionsContext);
		}

		/// <summary>
		/// 开始构建
		/// </summary>
		public void Run()
		{
			_buildContext.ClearAllContext();
			List<IBuildTask> pipeline = new List<IBuildTask>
			{
				new TaskPrepare(), //前期准备工作
				new TaskGetBuildMap(), //获取构建列表
				new TaskBuilding(), //开始构建
				new TaskEncryption(), //加密资源文件
				new TaskCheckCycle(), //检测循环依赖
				new TaskCreatePatchManifest(), //创建补丁文件
				new TaskCreateReadme(), //创建说明文件
				new TaskCopyUpdateFiles() //复制更新文件
			};
			BuildRunner.Run(pipeline, _buildContext);
		}

		/// <summary>
		/// 从输出目录加载补丁清单文件
		/// </summary>
		public static PatchManifest LoadPatchManifestFile(BuildParametersContext buildParameters)
		{
			string filePath = $"{buildParameters.OutputDirectory}/{PatchDefine.PatchManifestFileName}";
			if (File.Exists(filePath) == false)
				return new PatchManifest();

			string jsonData = FileUtility.ReadFile(filePath);
			return PatchManifest.Deserialize(jsonData);
		}

		/// <summary>
		/// 获取配置的输出目录
		/// </summary>
		public static string MakeOutputDirectory(string outputRoot, BuildTarget buildTarget)
		{
			return $"{outputRoot}/{buildTarget}/{PatchDefine.UnityManifestFileName}";
		}
	}
}