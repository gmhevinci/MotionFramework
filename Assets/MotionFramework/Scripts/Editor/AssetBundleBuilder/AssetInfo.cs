//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEditor;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 资源信息类
	/// </summary>
	public class AssetInfo
	{
		public string AssetPath { private set; get; }
		public System.Type AssetType { private set; get; }
		public bool IsCollectAsset { private set; get; }

		/// <summary>
		/// 被依赖次数
		/// </summary>
		public int DependCount = 0;

		/// <summary>
		/// AssetBundle标签
		/// </summary>
		public string AssetBundleLabel = null;

		/// <summary>
		/// AssetBundle变体
		/// </summary>
		public string AssetBundleVariant = null;

		/// <summary>
		/// 创建AssetBundleBuild类
		/// </summary>
		public AssetBundleBuild CreateAssetBundleBuild()
		{
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = AssetBundleLabel;
			build.assetBundleVariant = AssetBundleVariant;
			build.assetNames = new string[] { AssetPath };
			return build;
		}

		/// <summary>
		/// 获取AssetBundle的完整名称（包含后缀名）
		/// </summary>
		public string GetAssetBundleFullName()
		{
			if (string.IsNullOrEmpty(AssetBundleVariant))
				return AssetBundleLabel.ToLower();
			else
				return $"{AssetBundleLabel}.{AssetBundleVariant}".ToLower();
		}

		public AssetInfo(string assetPath)
		{
			AssetPath = assetPath;
			AssetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			IsCollectAsset = AssetBundleCollectorSettingData.IsCollectAsset(assetPath, AssetType);
		}
	}
}