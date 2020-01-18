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
	[CreateAssetMenu]
	public class CollectionSetting : ScriptableObject
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
			/// 以文件路径命名
			/// </summary>
			LabelByFilePath,

			/// <summary>
			/// 以文件名称命名
			/// </summary>
			LabelByFileName,

			/// <summary>
			/// 以文件夹路径命名（该文件夹下所有资源被打到一个AssetBundle文件里）
			/// </summary>
			LabelByFolderPath,

			/// <summary>
			/// 以文件夹名称命名（该文件夹下所有资源被打到一个AssetBundle文件里）
			/// </summary>
			LabelByFolderName,
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
		[SerializeField]
		public List<Wrapper> Elements = new List<Wrapper>();
	}
}