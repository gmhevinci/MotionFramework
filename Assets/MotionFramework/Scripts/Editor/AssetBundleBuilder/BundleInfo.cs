//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class BundleInfo
	{
		/// <summary>
		/// AssetBundle完整名称
		/// </summary>
		public string AssetBundleFullName { private set; get; }

		/// <summary>
		/// AssetBundle标签
		/// </summary>
		public string AssetBundleLabel { private set; get; }

		/// <summary>
		/// AssetBundle变体
		/// </summary>
		public string AssetBundleVariant { private set; get; }

		/// <summary>
		/// 包含的资源列表
		/// </summary>
		public readonly List<AssetInfo> Assets = new List<AssetInfo>();


		public BundleInfo(string bundleLabel, string bundleVariant)
		{
			AssetBundleLabel = bundleLabel;
			AssetBundleVariant = bundleVariant;
			AssetBundleFullName = AssetBundleBuilderHelper.MakeAssetBundleFullName(bundleLabel, bundleVariant);
		}

		/// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(AssetInfo assetInfo)
		{
			foreach (var asset in Assets)
			{
				if (asset.AssetPath == assetInfo.AssetPath)
					throw new System.Exception($"Asset is existed : {assetInfo.AssetPath}");
			}
			Assets.Add(assetInfo);
		}

		/// <summary>
		/// 获取包含的资源路径列表
		/// </summary>
		public string[] GetIncludeAssetPaths()
		{
			return Assets.Select(t => t.AssetPath).ToArray();
		}

		/// <summary>
		/// 获取主动收集的资源路径列表
		/// </summary>
		public string[] GetCollectAssetPaths()
		{
			return Assets.Where(t => t.IsCollectAsset && t.DontWriteAssetPath == false).Select(t => t.AssetPath).ToArray();
		}

		/// <summary>
		/// 创建AssetBundleBuild类
		/// </summary>
		public UnityEditor.AssetBundleBuild CreatePipelineBuild()
		{
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = AssetBundleLabel;
			build.assetBundleVariant = AssetBundleVariant;
			build.assetNames = GetBuildinAssetPaths();
			return build;
		}
		private string[] GetBuildinAssetPaths()
		{
			return Assets.Where(t => t.IsCollectAsset || t.DependCount > 0).Select(t => t.AssetPath).ToArray();
		}
	}
}