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
		/// <summary>
		/// 文件夹打包规则
		/// </summary>
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
		public class Wrapper
		{
			public string CollectDirectory = string.Empty;
			public ECollectRule CollectRule = ECollectRule.Collect;
			public string CollectorName = string.Empty;
		}

		/// <summary>
		/// 打包路径列表
		/// </summary>
		public List<Wrapper> Elements = new List<Wrapper>();
	}
}