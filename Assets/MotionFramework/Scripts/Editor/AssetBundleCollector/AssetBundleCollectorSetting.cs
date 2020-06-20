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
		public enum EFolderPackRule
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

		/// <summary>
		/// AssetBundle标签规则
		/// </summary>
		[Serializable]
		public enum EBundleLabelRule
		{
			None,

			/// <summary>
			/// 以文件路径作为标签名
			/// </summary>
			LabelByFilePath,

			/// <summary>
			/// 以文件夹路径作为标签名
			/// 注意：该文件夹下所有资源被打到一个AssetBundle文件里
			/// </summary>
			LabelByFolderPath,
		}

		[Serializable]
		public class Wrapper
		{
			public string FolderPath = string.Empty;
			public EFolderPackRule PackRule = EFolderPackRule.Collect;
			public EBundleLabelRule LabelRule = EBundleLabelRule.LabelByFilePath;
		}

		/// <summary>
		/// 打包路径列表
		/// </summary>
		public List<Wrapper> Elements = new List<Wrapper>();
	}
}