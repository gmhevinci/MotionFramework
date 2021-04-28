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
			var buildMap = context.GetContextObject<TaskGetBuildMap.BuildMapContext>();

			BuildLogger.Log($"开始构建......");
			BuildAssetBundleOptions opt = buildParameters.GetPiplineBuildOptions();
			AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(buildParameters.OutputDirectory, buildMap.GetPipelineBuilds(), opt, buildParameters.Parameters.BuildTarget);
			if (manifest == null)
				throw new Exception("构建过程中发生错误！");

			UnityManifestContext unityManifestContext = new UnityManifestContext();
			unityManifestContext.Manifest = manifest;
			context.SetContextObject(unityManifestContext);
		}
	}
}