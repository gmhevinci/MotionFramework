//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Editor
{
	public class CollectInfo
	{
		public string AssetPath { private set; get; }
		public bool DontWriteAssetPath { private set; get; }

		public CollectInfo(string assetPath, bool dontWriteAssetPath)
		{
			AssetPath = assetPath;
			DontWriteAssetPath = dontWriteAssetPath;
		}
	}
}