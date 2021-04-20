//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 检测资源之间是否有循环依赖
	/// </summary>
	internal class TaskCheckCycle : IBuildTask
	{
		void IBuildTask.Run(BuildContext context)
		{
			var unityManifestContext = context.GetContextObject<TaskBuilding.UnityManifestContext>();
			CheckCycleDepend(unityManifestContext.Manifest);
		}

		private void CheckCycleDepend(AssetBundleManifest unityManifest)
		{
			bool isFoundCycleDepend = false;
			List<string> visited = new List<string>(100);
			List<string> stack = new List<string>(100);
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();
			for (int i = 0; i < allAssetBundles.Length; i++)
			{
				var element = allAssetBundles[i];
				visited.Clear();
				stack.Clear();

				// 深度优先搜索检测有向图有无环路算法
				if (CheckCycle(unityManifest, element, visited, stack))
				{
					isFoundCycleDepend = true;
					Debug.LogError($"Found cycle assetbundle : {element}");
					foreach (var ele in stack)
					{
						Debug.LogWarning(ele);
					}
				}
			}

			if (isFoundCycleDepend)
				throw new Exception("Found cycle assetbundle, Please fix the error first.");
		}
		private bool CheckCycle(AssetBundleManifest unityManifest, string element, List<string> visited, List<string> stack)
		{
			if (visited.Contains(element) == false)
			{
				visited.Add(element);
				stack.Add(element);

				string[] depends = unityManifest.GetDirectDependencies(element);
				foreach (var dp in depends)
				{
					if (visited.Contains(dp) == false && CheckCycle(unityManifest, dp, visited, stack))
						return true;
					else if (stack.Contains(dp))
						return true;
				}
			}

			stack.Remove(element);
			return false;
		}
	}
}