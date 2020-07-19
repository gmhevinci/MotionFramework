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
			string filePath = AssetPathHelper.MakeStreamingLoadPath(PatchDefine.PatchManifestFileName);
			string url = AssetPathHelper.ConvertToWWWPath(filePath);
			WebDataRequest downloader = new WebDataRequest(url);
			downloader.DownLoad();
			yield return downloader;

			if (downloader.HasError())
			{
				downloader.ReportError();
				downloader.Dispose();
				throw new System.Exception($"Fatal error : Failed download file : {url}");
			}

			_patchManifest = PatchManifest.Deserialize(downloader.GetText());
			downloader.Dispose();
		}

		#region IBundleServices接口
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string manifestPath)
		{
			manifestPath = GetVariantManifestPath(_patchManifest, manifestPath);
			if (_patchManifest.Elements.TryGetValue(manifestPath, out PatchElement element))
			{
				// 直接从沙盒里加载
				string localPath = AssetPathHelper.MakeStreamingLoadPath(manifestPath);
				AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, localPath, string.Empty, element.MD5, element.SizeBytes, element.Version, element.IsEncrypted);
				return bundleInfo;
			}
			else
			{
				MotionLog.Warning($"Not found element in patch manifest : {manifestPath}");
				string localPath = AssetPathHelper.MakeStreamingLoadPath(manifestPath);
				AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, localPath);
				return bundleInfo;
			}
		}
		string[] IBundleServices.GetDirectDependencies(string assetBundleName)
		{
			return _patchManifest.GetDirectDependencies(assetBundleName);
		}
		string[] IBundleServices.GetAllDependencies(string assetBundleName)
		{
			return _patchManifest.GetAllDependencies(assetBundleName);
		}

		private string GetVariantManifestPath(PatchManifest patchManifest, string manifestPath)
		{
			if (_variantCollector == null)
				return manifestPath;

			if (patchManifest.HasVariant(manifestPath))
			{
				string variant = patchManifest.GetFirstVariant(manifestPath);
				return _variantCollector.TryGetVariantManifestPath(manifestPath, variant);
			}
			return manifestPath;
		}
		#endregion
	}
}