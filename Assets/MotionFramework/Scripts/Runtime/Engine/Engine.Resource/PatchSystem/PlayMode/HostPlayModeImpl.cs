//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Utility;
using MotionFramework.Network;

namespace MotionFramework.Resource
{
	internal class HostPlayModeImpl : IBundleServices
	{
		// 缓存器
		private PatchCache _cache;

		// 补丁清单
		private PatchManifest _appPatchManifest;
		private PatchManifest _localPatchManifest;

		// 参数相关
		private bool _clearCacheWhenDirty;
		private bool _ignoreResourceVersion;
		private EVerifyLevel _verifyLevel;
		private string _defaultHostServer;
		private string _fallbackHostServer;

		/// <summary>
		/// 清单更新结果
		/// </summary>
		public UpdateManifestResult ManifestResult { private set; get; }

		
		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAsync(bool clearCacheWhenDirty, bool ignoreResourceVersion,
			EVerifyLevel verifyLevel, string defaultHostServer, string fallbackHostServer)
		{
			_clearCacheWhenDirty = clearCacheWhenDirty;
			_ignoreResourceVersion = ignoreResourceVersion;
			_verifyLevel = verifyLevel;
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;

			// 如果缓存文件不存在
			if (PatchHelper.CheckSandboxCacheFileExist() == false)
			{
				_cache = new PatchCache();
				_cache.InitAppVersion(Application.version);
			}
			else
			{
				// 加载缓存
				_cache = PatchCache.LoadCache();

				// 每次启动时比对APP版本号是否一致	
				if (_cache.CacheAppVersion != Application.version)
				{
					MotionLog.Warning($"Cache is dirty ! Cache app version is {_cache.CacheAppVersion}, Current app version is {Application.version}");

					// 注意：在覆盖安装的时候，会保留APP沙盒目录，可以选择清空缓存目录
					if (_clearCacheWhenDirty)
					{
						_cache.ClearCache();
					}

					// 注意：一定要删除清单文件
					PatchHelper.DeleteSandboxPatchManifestFile();
					_cache.InitAppVersion(Application.version);
				}
			}

			// 加载APP内的补丁清单
			MotionLog.Log($"Load application patch manifest.");
			{
				string filePath = AssetPathHelper.MakeStreamingLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
				string url = AssetPathHelper.ConvertToWWWPath(filePath);
				WebGetRequest downloader = new WebGetRequest(url);
				downloader.SendRequest();
				yield return downloader;

				if (downloader.HasError())
				{
					downloader.ReportError();
					downloader.Dispose();
					throw new Exception($"Fatal error : Failed load application patch manifest file : {url}");
				}

				// 解析补丁清单
				string jsonData = downloader.GetText();
				_appPatchManifest = PatchManifest.Deserialize(jsonData);
				_localPatchManifest = _appPatchManifest;
				downloader.Dispose();
			}

			// 加载沙盒内的补丁清单	
			if (PatchHelper.CheckSandboxPatchManifestFileExist())
			{
				MotionLog.Log($"Load sandbox patch manifest.");
				string filePath = AssetPathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
				string jsonData = File.ReadAllText(filePath);
				_localPatchManifest = PatchManifest.Deserialize(jsonData);
			}
		}

		/// <summary>
		/// 更新补丁清单
		/// </summary>
		public IEnumerator UpdatePatchManifestAsync(int updateResourceVersion, int timeout)
		{
			if (ManifestResult == null)
				ManifestResult = new UpdateManifestResult();

			if (_ignoreResourceVersion && updateResourceVersion > 0)
			{
				MotionLog.Warning($"Update resource version {updateResourceVersion} is invalid when ignore resource version.");
			}

			ManifestResult.Reset();
			ManifestResult.RequestCount++;

			MotionLog.Log($"Update patch manifest : update resource version is  {updateResourceVersion}");

			// 从远端请求补丁清单文件的哈希值，并比对沙盒内的补丁清单文件的哈希值
			{
				string webURL = GetPatchManifestRequestURL(updateResourceVersion, ResourceSettingData.Setting.PatchManifestHashFileName);
				MotionLog.Log($"Beginning to request patch manifest hash : {webURL}");
				WebGetRequest download = new WebGetRequest(webURL);
				download.SendRequest(timeout);
				yield return download;

				// Check fatal
				if (download.HasError())
				{
					ManifestResult.Error = download.GetError();
					ManifestResult.States = UpdateManifestResult.EStates.Failed;
					download.Dispose();
					yield break;
				}

				// 获取补丁清单文件的哈希值
				string patchManifestHash = download.GetText();
				download.Dispose();

				// 如果补丁清单文件的哈希值相同
				string currentFileHash = PatchHelper.GetSandboxPatchManifestFileHash();
				if (currentFileHash == patchManifestHash)
				{
					ManifestResult.States = UpdateManifestResult.EStates.Succeed;
					MotionLog.Log($"Patch manifest file hash is not change : {patchManifestHash}");
					yield break;
				}
				else
				{
					MotionLog.Log($"Patch manifest hash is change : {patchManifestHash} -> {currentFileHash}");
				}
			}

			// 从远端请求补丁清单
			{
				string webURL = GetPatchManifestRequestURL(updateResourceVersion, ResourceSettingData.Setting.PatchManifestFileName);
				MotionLog.Log($"Beginning to request patch manifest : {webURL}");
				WebGetRequest download = new WebGetRequest(webURL);
				download.SendRequest(timeout);
				yield return download;

				// Check fatal
				if (download.HasError())
				{
					ManifestResult.Error = download.GetError();
					ManifestResult.States = UpdateManifestResult.EStates.Failed;
					download.Dispose();
					yield break;
				}

				// 解析补丁清单			
				ParseAndSaveRemotePatchManifest(download.GetText());
				ManifestResult.States = UpdateManifestResult.EStates.Succeed;
				download.Dispose();
			}
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (_localPatchManifest == null)
				return 0;
			return _localPatchManifest.ResourceVersion;
		}

		/// <summary>
		/// 获取内置资源标记列表
		/// </summary>
		public string[] GetManifestBuildinTags()
		{
			if (_localPatchManifest == null)
				return new string[0];
			return _localPatchManifest.GetBuildinTags();
		}

		/// <summary>
		/// 创建补丁下载器
		/// </summary>
		/// <param name="dlcTags">DLC标记列表</param>
		/// <param name="fileLoadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public PatchDownloader CreateDLCDownloader(string[] dlcTags, int fileLoadingMaxNumber, int failedTryAgain)
		{
			List<PatchBundle> downloadList = GetPatchDownloadList(dlcTags);
			PatchDownloader downlader = new PatchDownloader(this, downloadList, fileLoadingMaxNumber, failedTryAgain);
			return downlader;
		}

		/// <summary>
		/// 获取补丁下载列表
		/// </summary>
		private List<PatchBundle> GetPatchDownloadList(string[] dlcTags)
		{
			List<PatchBundle> downloadList = new List<PatchBundle>(1000);
			foreach (var patchBundle in _localPatchManifest.BundleList)
			{
				// 忽略缓存资源
				if (_cache.Contains(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (_appPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				// 如果是纯内置资源，则统一下载
				// 注意：可能是新增的或者变化的内置资源
				// 注意：可能是由热更资源转换的内置资源
				if (patchBundle.IsPureBuildin())
				{
					downloadList.Add(patchBundle);
				}
				else
				{
					// 查询DLC资源
					if (patchBundle.HasTag(dlcTags))
					{
						downloadList.Add(patchBundle);
					}
				}
			}

			return CacheAndFilterDownloadList(downloadList);
		}


		// 检测下载内容的完整性
		internal bool CheckContentIntegrity(string bundleName)
		{
			if (_localPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				return CheckContentIntegrity(patchBundle);
			}
			else
			{
				MotionLog.Warning($"Not found check content file in local patch manifest : {bundleName}");
				return false;
			}
		}
		internal bool CheckContentIntegrity(PatchBundle patchBundle)
		{
			return CheckContentIntegrity(patchBundle.Hash, patchBundle.CRC, patchBundle.SizeBytes);
		}
		private bool CheckContentIntegrity(string hash, string crc, long size)
		{
			string filePath = PatchHelper.MakeSandboxCacheFilePath(hash);
			if (File.Exists(filePath) == false)
				return false;

			// 校验沙盒里的补丁文件
			if (_verifyLevel == EVerifyLevel.Size)
			{
				long fileSize = FileUtility.GetFileSize(filePath);
				return fileSize == size;
			}
			else if (_verifyLevel == EVerifyLevel.CRC)
			{
				string fileCRC = HashUtility.FileCRC32(filePath);
				return fileCRC == crc;
			}
			else
			{
				throw new NotImplementedException(_verifyLevel.ToString());
			}
		}

		// 缓存系统相关
		internal void CacheDownloadPatchFile(string bundleName)
		{
			if (_localPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				MotionLog.Log($"Cache download web file : {patchBundle.BundleName} Version : {patchBundle.Version} Hash : {patchBundle.Hash}");
				_cache.CacheDownloadPatchFile(patchBundle.Hash);
			}
			else
			{
				MotionLog.Warning($"Not found bundle in local patch manifest : {bundleName}");
			}
		}
		internal void CacheDownloadPatchFiles(List<PatchBundle> downloadList)
		{
			List<string> hashList = new List<string>(downloadList.Count);
			foreach (var patchBundle in downloadList)
			{
				MotionLog.Log($"Cache download web file : {patchBundle.BundleName} Version : {patchBundle.Version} Hash : {patchBundle.Hash}");
				hashList.Add(patchBundle.Hash);
			}
			_cache.CacheDownloadPatchFiles(hashList);
		}
		private List<PatchBundle> CacheAndFilterDownloadList(List<PatchBundle> downloadList)
		{
			// 检测文件是否已经下载完毕
			// 注意：如果玩家在加载过程中强制退出，下次再进入的时候跳过已经加载的文件
			List<PatchBundle> cacheList = new List<PatchBundle>();
			for (int i = downloadList.Count - 1; i >= 0; i--)
			{
				var patchBundle = downloadList[i];
				if (CheckContentIntegrity(patchBundle))
				{
					cacheList.Add(patchBundle);
					downloadList.RemoveAt(i);
				}
			}

			// 缓存已经下载的有效文件
			if (cacheList.Count > 0)
				CacheDownloadPatchFiles(cacheList);

			return downloadList;
		}

		// 补丁清单相关
		private string GetPatchManifestRequestURL(int updateResourceVersion, string fileName)
		{
			string url;

			// 轮流返回请求地址
			if (ManifestResult.RequestCount % 2 == 0)
				url = GetPatchDownloadFallbackURL(updateResourceVersion, fileName);
			else
				url = GetPatchDownloadURL(updateResourceVersion, fileName);

			// 注意：在URL末尾添加时间戳
			if (_ignoreResourceVersion)
				url = $"{url}?{System.DateTime.UtcNow.Ticks}";

			return url;
		}
		private void ParseAndSaveRemotePatchManifest(string content)
		{
			_localPatchManifest = PatchManifest.Deserialize(content);

			// 注意：这里会覆盖掉沙盒内的补丁清单文件
			MotionLog.Log("Save remote patch manifest file.");
			string savePath = AssetPathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
			PatchManifest.Serialize(savePath, _localPatchManifest);
		}

		// WEB相关
		internal string GetPatchDownloadURL(int resourceVersion, string fileName)
		{
			if (_ignoreResourceVersion)
				return $"{_defaultHostServer}/{fileName}";
			else
				return $"{_defaultHostServer}/{resourceVersion}/{fileName}";
		}
		internal string GetPatchDownloadFallbackURL(int resourceVersion, string fileName)
		{
			if (_ignoreResourceVersion)
				return $"{_fallbackHostServer}/{fileName}";
			else
				return $"{_fallbackHostServer}/{resourceVersion}/{fileName}";
		}

		#region IBundleServices接口
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				return new AssetBundleInfo(string.Empty, string.Empty);

			if (_localPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				// 查询APP资源
				if (_appPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
					{
						string appLoadPath = AssetPathHelper.MakeStreamingLoadPath(appPatchBundle.Hash);
						AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, appLoadPath, appPatchBundle.Version, appPatchBundle.IsEncrypted, appPatchBundle.IsRawFile);
						return bundleInfo;
					}
				}

				// 查询缓存资源
				// 注意：如果沙盒内缓存文件不存在，那么将会从服务器下载
				string sandboxLoadPath = PatchHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
				if (_cache.Contains(patchBundle.Hash))
				{
					AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, sandboxLoadPath, patchBundle.Version, patchBundle.IsEncrypted, patchBundle.IsRawFile);
					return bundleInfo;
				}
				else
				{
					string remoteURL = GetPatchDownloadURL(patchBundle.Version, patchBundle.Hash);
					string remoteFallbackURL = GetPatchDownloadFallbackURL(patchBundle.Version, patchBundle.Hash);
					AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, sandboxLoadPath, remoteURL, remoteFallbackURL, patchBundle.Version, patchBundle.IsEncrypted, patchBundle.IsRawFile);
					return bundleInfo;
				}
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
			bool result = CheckContentIntegrity(bundleName);
			if (result)
			{
				CacheDownloadPatchFile(bundleName);
			}
			return result;
		}
		string IBundleServices.GetAssetBundleName(string assetPath)
		{
			return _localPatchManifest.GetAssetBundleName(assetPath);
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return _localPatchManifest.GetAllDependencies(assetPath);
		}
		#endregion
	}
}