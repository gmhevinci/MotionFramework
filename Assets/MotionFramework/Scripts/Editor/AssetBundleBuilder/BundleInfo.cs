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
		/// 是否包含指定资源
		/// </summary>
		public bool IsContainsAsset(string assetPath)
		{
			foreach (var assetInfo in Assets)
			{
				if (assetInfo.AssetPath == assetPath)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(AssetInfo assetInfo)
		{
			if(IsContainsAsset(assetInfo.AssetPath))
				throw new System.Exception($"Asset is existed : {assetInfo.AssetPath}");

			Assets.Add(assetInfo);
		}

		/// <summary>
		/// 获取资源标记列表
		/// </summary>
		public string[] GetAssetTags()
		{
			List<string> result = new List<string>(Assets.Count);
			foreach(var assetInfo in Assets)
			{
				foreach(var assetTag in assetInfo.AssetTags)
				{
					if (result.Contains(assetTag) == false)
						result.Add(assetTag);
				}
			}
			return result.ToArray();
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
			return Assets.Where(t => t.IsCollectAsset).Select(t => t.AssetPath).ToArray();
		}

		/// <summary>
		/// 获取构建的资源路径列表
		/// </summary>
		public string[] GetBuildinAssetPaths()
		{
			// 注意：对于非主动收集的零依赖资源，不会出现在AssetBundle的Assets列表里。
			// Unity在构建AssetBundle的时候，也会根据主动收集资源的依赖关系将它们打包到文件里。
			return Assets.Where(t => t.IsCollectAsset || t.DependCount > 0).Select(t => t.AssetPath).ToArray();
		}

		/// <summary>
		/// 获取主动收集的资源信息列表
		/// </summary>
		public AssetInfo[] GetCollectAssetInfos()
		{
			return Assets.Where(t => t.IsCollectAsset).ToArray();
		}

		/// <summary>
		/// 创建AssetBundleBuild类
		/// </summary>
		public UnityEditor.AssetBundleBuild CreatePipelineBuild()
		{
			// 注意：我们不在支持AssetBundle的变种机制
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = AssetBundleFullName;
			build.assetBundleVariant = string.Empty;
			build.assetNames = GetBuildinAssetPaths();
			return build;
		}
	}
}