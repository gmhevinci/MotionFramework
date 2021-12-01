//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Network;

namespace MotionFramework.Resource
{
	public sealed class OfflinePlayModeImpl : IBundleServices
	{
		private PatchManifest _patchManifest;
		
		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAsync()
		{
			// 解析APP里的补丁清单
			string filePath = AssetPathHelper.MakeStreamingLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
			string url = AssetPathHelper.ConvertToWWWPath(filePath);
			WebGetRequest downloader = new WebGetRequest(url);
			downloader.SendRequest();
			yield return downloader;

			if (downloader.HasError())
			{
				downloader.ReportError();
				downloader.Dispose();
				throw new System.Exception($"Fatal error : Failed load application patch manifest file : {url}");
			}

			_patchManifest = PatchManifest.Deserialize(downloader.GetText());
			downloader.Dispose();
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (_patchManifest == null)
				return 0;			
			return _patchManifest.ResourceVersion;
		}

		/// <summary>
		/// 获取内置资源标记列表
		/// </summary>
		public string[] GetManifestBuildinTags()
		{
			if (_patchManifest == null)
				return new string[0];
			return _patchManifest.GetBuildinTags();
		}

		#region IBundleServices接口
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				return new AssetBundleInfo(string.Empty, string.Empty);

			if (_patchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				string localPath = AssetPathHelper.MakeStreamingLoadPath(patchBundle.Hash);
				AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, localPath, patchBundle.Version, patchBundle.IsEncrypted, patchBundle.IsRawFile);
				return bundleInfo;
			}
			else
			{
				MotionLog.Warning($"Not found bundle in patch manifest : {bundleName}");
				AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, string.Empty);
				return bundleInfo;
			}
		}
		bool IBundleServices.CacheDownloadFile(string bundleName)
		{
			throw new NotImplementedException();
		}
		string IBundleServices.GetAssetBundleName(string assetPath)
		{
			return _patchManifest.GetAssetBundleName(assetPath);
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return _patchManifest.GetAllDependencies(assetPath);
		}
		#endregion
	}
}