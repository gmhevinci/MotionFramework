//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MotionFramework.Resource;
using MotionFramework.IO;

namespace MotionFramework.Patch
{
	public sealed class LocalBundleServices : IBundleServices
	{
		private VariantCollector _variantCollector = new VariantCollector();

		/// <summary>
		/// 适合单机游戏的资源文件服务接口类
		/// </summary>
		public LocalBundleServices()
		{
		}

		/// <summary>
		/// 适合单机游戏的资源文件服务接口类
		/// </summary>
		/// <param name="variantRules">变体规则</param>
		public LocalBundleServices(List<VariantRule> variantRules)
		{
			if (variantRules != null)
			{
				foreach (var variantRule in variantRules)
				{
					_variantCollector.RegisterVariantRule(variantRule.VariantGroup, variantRule.TargetVariant);
				}
			}
		}

		#region IBundleServices接口
		private string _cachedLocationRoot;
		private AssetBundleManifest _unityManifest;
		private AssetBundleManifest LoadUnityManifest()
		{
			IBundleServices bundleServices = this as IBundleServices;
			string loadPath = bundleServices.GetAssetBundleLoadPath(PatchDefine.UnityManifestFileName);
			AssetBundle bundle = AssetBundle.LoadFromFile(loadPath);
			if (bundle == null)
				return null;

			AssetBundleManifest result = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			bundle.Unload(false);
			return result;
		}

		string IBundleServices.ConvertLocationToManifestPath(string location, string variant)
		{
			if (_cachedLocationRoot == null)
			{
				if (string.IsNullOrEmpty(AssetSystem.LocationRoot))
					throw new System.Exception($"{nameof(AssetSystem.LocationRoot)} is null or empty.");
				_cachedLocationRoot = AssetSystem.LocationRoot.ToLower();
			}

			if (string.IsNullOrEmpty(variant))
				throw new System.Exception($"Variant is null or empty: {location}");

			return StringFormat.Format("{0}/{1}.{2}", _cachedLocationRoot, location.ToLower(), variant.ToLower());
		}
		string IBundleServices.GetAssetBundleLoadPath(string manifestPath)
		{
			// 尝试获取变体资源清单路径
			manifestPath = _variantCollector.TryGetVariantManifestPath(manifestPath);

			// 从流文件夹内加载所有文件
			return AssetPathHelper.MakeStreamingLoadPath(manifestPath);		
		}
		string[] IBundleServices.GetDirectDependencies(string assetBundleName)
		{
			if (_unityManifest == null)
				_unityManifest = LoadUnityManifest();
			return _unityManifest.GetDirectDependencies(assetBundleName);
		}
		string[] IBundleServices.GetAllDependencies(string assetBundleName)
		{
			if (_unityManifest == null)
				_unityManifest = LoadUnityManifest();
			return _unityManifest.GetAllDependencies(assetBundleName);
		}
		#endregion
	}
}