//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 张飞涛 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MotionFramework.Editor
{
	public class SearchAll : ISearchFilter
	{
		public bool FilterAsset(string assetPath)
		{
			return true;
		}
	}

	public class SearchScene : ISearchFilter
	{
		public bool FilterAsset(string assetPath)
		{
			return Path.GetExtension(assetPath) == ".unity";
		}
	}
	
	public class SearchPrefab : ISearchFilter
	{
		public bool FilterAsset(string assetPath)
		{
			return Path.GetExtension(assetPath) == ".prefab";
		}
	}

	public class SearchSprite : ISearchFilter
	{
		public bool FilterAsset(string assetPath)
		{
			if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(Sprite))
				return true;
			else
				return false;
		}
	}
}