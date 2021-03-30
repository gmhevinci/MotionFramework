//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
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
			/// <summary>
			/// 收集路径
			/// </summary>
			public string CollectDirectory = string.Empty;

			/// <summary>
			/// 标签的类名
			/// </summary>
			public string LabelClassName = string.Empty;

			/// <summary>
			/// 过滤器的类名
			/// </summary>
			public string FilterClassName = string.Empty;
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