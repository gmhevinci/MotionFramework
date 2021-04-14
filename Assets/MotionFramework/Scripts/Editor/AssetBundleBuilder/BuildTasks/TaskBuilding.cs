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
using UnityEngine;

namespace MotionFramework.Editor
{
	internal class TaskBuilding : IBuildTask
	{
		public class UnityManifestContext : IContextObject
		{
			public AssetBundleManifest Manifest;
		}

		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();
			var buildOptions = context.GetContextObject<AssetBundleBuilder.BuildOptionsContext>();
			var buildMap = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();

			BuildLogger.Log($"开始构建......");
			BuildAssetBundleOptions opt = MakeBuildOptions(buildOptions);
			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(buildParameters.OutputDirectory, buildMap.GetPipelineBuilds(), opt, buildParameters.BuildTarget);
			if (manifest == null)
				throw new Exception("构建过程中发生错误！");

			UnityManifestContext unityManifestContext = new UnityManifestContext();
			unityManifestContext.Manifest = manifest;
			context.SetContextObject(unityManifestContext);
		}

		/// <summary>
		/// 获取构建选项
		/// </summary>
		public BuildAssetBundleOptions MakeBuildOptions(AssetBundleBuilder.BuildOptionsContext buildOptions)
		{
			// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
			// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

			BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
			opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

			if (buildOptions.CompressOption == ECompressOption.Uncompressed)
				opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
			else if (buildOptions.CompressOption == ECompressOption.ChunkBasedCompressionLZ4)
				opt |= BuildAssetBundleOptions.ChunkBasedCompression;

			if (buildOptions.IsForceRebuild)
				opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
			if (buildOptions.IsAppendHash)
				opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName; //Append the hash to the assetBundle name
			if (buildOptions.IsDisableWriteTypeTree)
				opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
			if (buildOptions.IsIgnoreTypeTreeChanges)
				opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

			return opt;
		}
	}
}