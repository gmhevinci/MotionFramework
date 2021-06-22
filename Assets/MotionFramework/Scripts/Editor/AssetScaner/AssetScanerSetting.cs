//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Editor
{
	[CreateAssetMenu]
	public class AssetScanerSetting : ScriptableObject
	{
		[Serializable]
		public class Wrapper
		{
			public string ScanerDirectory = string.Empty;
			public string ScanerName = string.Empty;
		}

		/// <summary>
		/// 路径列表
		/// </summary>
		public List<Wrapper> Elements = new List<Wrapper>();
	}
}