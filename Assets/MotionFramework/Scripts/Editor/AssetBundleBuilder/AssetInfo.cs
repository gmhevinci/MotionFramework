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
		/// 不写入资源路径信息到清单文件
		/// </summary>
		public bool DontWriteAssetPath = false;

		/// <summary>
		/// 资源标记列表
		/// </summary>
		public List<string> AssetTags = new List<string>();

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
		/// 添加资源标记
		/// </summary>
		public void AddAssetTags(List<string> tags)
		{
			foreach(var tag in tags)
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