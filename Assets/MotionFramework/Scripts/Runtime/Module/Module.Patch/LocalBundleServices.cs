//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.IO;
using MotionFramework.Resource;
using MotionFramework.Network;

namespace MotionFramework.Patch
{
	public sealed class LocalBundleServices : IBundleServices
	{
		private VariantCollector _variantCollector;
		private PatchManifest _patchManifest;

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
			_variantCollector = new VariantCollector();
			if (variantRules != null)
			{
				foreach (var variantRule in variantRules)
				{
					_variantCollector.RegisterVariantRule(variantRule.VariantGroup, variantRule.TargetVariant);
				}
			}
		}


		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAsync(bool simulationOnEditor)
		{
			if (simulationOnEditor)
				yield break;

			// 解析APP里的补丁清单
			string filePath = AssetPathHelper.MakeStreamingLoadPath(PatchDefine.PatchManifestBytesFileName);
			string url = AssetPathHelper.ConvertToWWWPath(filePath);
			WebDataRequest downloader = new WebDataRequest(url);
			yield return downloader.DownLoad();

			if (downloader.States == EWebRequestStates.Success)
			{
				_patchManifest = new PatchManifest();
				_patchManifest.Parse(downloader.GetData());
				downloader.Dispose();
			}
			else
			{
				throw new System.Exception($"Fatal error : Failed download file : {url}");
			}
		}

		#region IBundleServices接口
		private string _cachedLocationRoot;
		private AssetBundleManifest _unityManifest;
		private AssetBundleManifest LoadUnityManifest()
		{
			IBundleServices bundleServices = this;
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
			if (_patchManifest.Elements.TryGetValue(manifestPath, out PatchElement element))
			{
				// 如果是变体资源
				if (_variantCollector != null)
				{
					string variant = element.GetFirstVariant();
					if (string.IsNullOrEmpty(variant) == false)
						manifestPath = _variantCollector.TryGetVariantManifestPath(manifestPath, variant);
				}

				// 直接从沙盒里加载
				return AssetPathHelper.MakeStreamingLoadPath(manifestPath);
			}
			else
			{
				MotionLog.Warning($"Not found element in patch manifest : {manifestPath}");
				return AssetPathHelper.MakeStreamingLoadPath(manifestPath);
			}
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