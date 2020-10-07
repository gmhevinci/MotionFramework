//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	/// <summary>
	/// Downloadable Content
	/// </summary>
	public class DLContent
	{
		[Serializable]
		public class DLCElement
		{
			/// <summary>
			/// 资源包名称
			/// </summary>
			public string BundleName;

			/// <summary>
			/// DLC标签
			/// </summary>
			public string DLCLabel;
		}

		/// <summary>
		/// 默认的DLC标签
		/// 注意：所有DLC资源共享的标签
		/// </summary>
		public string DefaultLabel = string.Empty;

		/// <summary>
		/// DLC列表
		/// </summary>
		public List<DLCElement> Elements = new List<DLCElement>();

		/// <summary>
		/// 检测是否包含
		/// </summary>
		public bool IsContains(string bundleName)
		{
			foreach (var element in Elements)
			{
				if (element.BundleName == bundleName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 获取AssetBundle的DLC标签
		/// </summary>
		public string GetAssetBundleDLCLabel(string manifestPath)
		{
			foreach(var element in Elements)
			{
				if (element.BundleName == manifestPath)
					return element.DLCLabel;
			}
			return string.Empty;
		}


		/// <summary>
		/// 序列化（JSON文本格式）
		/// </summary>
		public static void Serialize(DLContent dlc, string filePath)
		{
			string jsonData = UnityEngine.JsonUtility.ToJson(dlc, true);
			FileUtility.CreateFile(filePath, jsonData);
		}

		/// <summary>
		/// 反序列化（JSON文本格式）
		/// </summary>
		public static DLContent Deserialize(string filePath)
		{
			string jsonData = FileUtility.ReadFile(filePath);
			return UnityEngine.JsonUtility.FromJson<DLContent>(jsonData);
		}
	}
}