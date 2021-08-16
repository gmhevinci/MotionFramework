//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Editor
{
	public class AssetCollectInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 资源标记列表
		/// </summary>
		public List<string> AssetTags { private set; get; }

		public AssetCollectInfo(string assetPath, List<string> assetTags)
		{
			AssetPath = assetPath;
			AssetTags = assetTags;
		}
	}
}