//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Editor
{
	public static class BuildLogger
	{
		public static void Log(string info)
		{
			Debug.Log($"[BuildPatch] {info}");
		}

		public static void Warning(string info)
		{
			Debug.LogWarning($"[BuildPatch] {info}");
		}
	}
}