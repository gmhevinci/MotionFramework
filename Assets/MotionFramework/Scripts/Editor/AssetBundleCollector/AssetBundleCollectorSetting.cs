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
		public enum ECollectRule
		{
			/// <summary>
			/// 收集该文件夹
			/// </summary>
			Collect,

			/// <summary>
			/// 忽略该文件夹
			/// </summary>
			Ignore,
		}

		[Serializable]
		public class Collector
		{
			/// <summary>
			/// 收集路径
			/// </summary>
			public string CollectDirectory = string.Empty;

			/// <summary>
			/// 收集规则
			/// </summary>
			public ECollectRule CollectRule = ECollectRule.Collect;

			/// <summary>
			/// 收集器的类名
			/// </summary>
			public string CollectorName = string.Empty;
		}

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