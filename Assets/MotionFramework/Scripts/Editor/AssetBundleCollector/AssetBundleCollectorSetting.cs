//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.IO;

namespace MotionFramework.Editor
{
	public class AssetBundleCollectorSetting : ScriptableObject
	{
		[Serializable]
		public class Collector
		{
			public string CollectDirectory = string.Empty;
			public string PackRuleName = string.Empty;
			public string FilterRuleName = string.Empty;
			public bool DontWriteAssetPath = false;
			public string AssetTags = string.Empty;

			/// <summary>
			/// 获取资源标记列表
			/// </summary>
			public List<string> GetAssetTags()
			{
				return StringConvert.StringToStringList(AssetTags, ';');
			}

			public override string ToString()
			{
				return $"Directory : {CollectDirectory} | {PackRuleName} | {FilterRuleName} | {DontWriteAssetPath} | {AssetTags}";
			}
		}

		/// <summary>
		/// 是否收集全路径的着色器
		/// </summary>
		public bool IsCollectAllShaders = false;

		/// <summary>
		/// 收集的着色器Bundle名称
		/// </summary>
		public string ShadersBundleName = "myshaders";

		/// <summary>
		/// 收集列表
		/// </summary>
		public List<Collector> Collectors = new List<Collector>();
	}
}