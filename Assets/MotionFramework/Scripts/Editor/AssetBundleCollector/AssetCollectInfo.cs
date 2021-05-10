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

		/// <summary>
		/// 对于那些不依赖于代码加载的主动收集资源，可以禁止写入资源路径信息到清单文件
		/// </summary>
		public bool DontWriteAssetPath { private set; get; }

		public AssetCollectInfo(string assetPath, List<string> assetTags, bool dontWriteAssetPath)
		{
			AssetPath = assetPath;
			AssetTags = assetTags;
			DontWriteAssetPath = dontWriteAssetPath;
		}
	}
}