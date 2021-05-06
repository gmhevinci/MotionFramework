//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
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
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// AssetBundle标签
		/// </summary>
		public string AssetBundleLabel { private set; get; }

		/// <summary>
		/// AssetBundle变体
		/// </summary>
		public string AssetBundleVariant { private set; get; }

		/// <summary>
		/// 收集标记
		/// </summary>
		public bool IsCollectAsset = false;

		/// <summary>
		/// 不写入资源路径信息到清单文件
		/// </summary>
		public bool DontWriteAssetPath = false;

		/// <summary>
		/// 被依赖次数
		/// </summary>
		public int DependCount = 0;


		public AssetInfo(string assetPath)
		{
			AssetPath = assetPath;
		}

		/// <summary>
		/// 设置资源包的标签和变种
		/// </summary>
		public void SetBundleLabelAndVariant(string bundleLabel, string bundleVariant)
		{
			AssetBundleLabel = bundleLabel;
			AssetBundleVariant = bundleVariant;
		}

		/// <summary>
		/// 获取AssetBundle的完整名称
		/// </summary>
		public string GetAssetBundleFullName()
		{
			return AssetBundleBuilderHelper.MakeAssetBundleFullName(AssetBundleLabel, AssetBundleVariant);
		}
	}
}