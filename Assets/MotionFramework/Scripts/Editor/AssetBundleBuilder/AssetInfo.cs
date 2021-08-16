//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;

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
		/// 是否为主动收集资源
		/// </summary>
		public bool IsCollectAsset = false;

		/// <summary>
		/// 资源标记列表
		/// </summary>
		public List<string> AssetTags = new List<string>();

		/// <summary>
		/// 被依赖次数
		/// </summary>
		public int DependCount = 0;

		/// <summary>
		/// 依赖的所有资源信息
		/// </summary>
		public List<AssetInfo> AllDependAssetInfos { private set; get; } = null;


		public AssetInfo(string assetPath)
		{
			AssetPath = assetPath;
		}
		
		/// <summary>
		/// 设置所有依赖的资源
		/// </summary>
		public void SetAllDependAssetInfos(List<AssetInfo> dependAssetInfos)
		{
			if(AllDependAssetInfos != null)
				throw new System.Exception("Should never get here !");

			AllDependAssetInfos = dependAssetInfos;
		}

		/// <summary>
		/// 设置资源包的标签和变种
		/// </summary>
		public void SetBundleLabelAndVariant(string bundleLabel, string bundleVariant)
		{
			if (string.IsNullOrEmpty(AssetBundleLabel) == false || string.IsNullOrEmpty(AssetBundleVariant) == false)
				throw new System.Exception("Should never get here !");

			AssetBundleLabel = bundleLabel;
			AssetBundleVariant = bundleVariant;
		}

		/// <summary>
		/// 添加资源标记
		/// </summary>
		public void AddAssetTags(List<string> tags)
		{
			foreach (var tag in tags)
			{
				if (AssetTags.Contains(tag) == false)
				{
					AssetTags.Add(tag);
				}
			}
		}

		/// <summary>
		/// 获取AssetBundle的完整名称
		/// </summary>
		public string GetAssetBundleFullName()
		{
			if (string.IsNullOrEmpty(AssetBundleLabel) || string.IsNullOrEmpty(AssetBundleVariant))
				throw new System.ArgumentNullException();

			return AssetBundleBuilderHelper.MakeAssetBundleFullName(AssetBundleLabel, AssetBundleVariant);
		}
	}
}