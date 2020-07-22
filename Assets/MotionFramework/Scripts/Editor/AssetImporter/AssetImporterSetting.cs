//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Editor
{
	[CreateAssetMenu]
	public class AssetImporterSetting : ScriptableObject
	{
		[Serializable]
		public class Wrapper
		{
			public string ProcessDirectory = string.Empty;
			public string ProcessorName = string.Empty;
		}

		/// <summary>
		/// 开关
		/// </summary>
		public bool Toggle = true;

		/// <summary>
		/// 路径列表
		/// </summary>
		public List<Wrapper> Elements = new List<Wrapper>();
	}
}