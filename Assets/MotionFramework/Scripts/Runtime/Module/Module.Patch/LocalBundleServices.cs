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
		private PatchManifest _patchManifest;

		/// <summary>
		/// 适合单机游戏的资源文件服务接口类
		/// </summary>
		public LocalBundleServices()
		{
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
			WebGetRequest downloader = new WebGetRequest(url);
			downloader.SendRequest();
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
		bool IBundleServices.CheckContentIntegrity(string bundleName)
		{
			throw new NotImplementedException();
		}
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string bundleName)
		{
			if(string.IsNullOrEmpty(bundleName))
				return new AssetBundleInfo(string.Empty, string.Empty);

			if (_patchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				string localPath = AssetPathHelper.MakeStreamingLoadPath(patchBundle.Hash);
				AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, localPath, patchBundle.Version, patchBundle.IsEncrypted);
				return bundleInfo;
			}
			else
			{
				MotionLog.Warning($"Not found bundle in patch manifest : {bundleName}");
				AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, string.Empty);
				return bundleInfo;
			}
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