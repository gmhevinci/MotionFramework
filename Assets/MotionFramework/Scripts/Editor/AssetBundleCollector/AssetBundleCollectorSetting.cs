//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Editor
{
	public class AssetBundleCollectorSetting : ScriptableObject
	{
		[Serializable]
		public class Collector
		{
			public string CollectDirectory = string.Empty;
			public string PackRuleClassName = string.Empty;
			public string FilterRuleClassName = string.Empty;
			public bool DontWriteAssetPath = false;

			public override string ToString()
			{
				return $"Directory : {CollectDirectory} | {PackRuleClassName} | {FilterRuleClassName} | {DontWriteAssetPath}";
			}
		}

		/// <summary>
		/// 是否收集全路径的着色器
		/// </summary>
		public bool IsCollectAllShaders = false;

		/// <summary>
		/// 收集的着色器Bundle名称
		/// </summary>
		public string ShadersBundleName = "shaders";

		/// <summary>
		/// 收集列表
		/// </summary>
		public List<Collector> Collectors = new List<Collector>();

		/// <summary>
		/// DLC文件列表
		/// </summary>
		public List<string> DLCFiles = new List<string>();
	}
}