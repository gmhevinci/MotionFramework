//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Utility;

namespace MotionFramework.Resource
{
	internal class HostPlayModeImpl : IBundleServices
	{
		// 缓存器
		internal PatchCache Cache;

		// 补丁清单
		internal PatchManifest AppPatchManifest;
		internal PatchManifest LocalPatchManifest;

		// 参数相关
		internal bool ClearCacheWhenDirty { private set; get; }
		internal bool IgnoreResourceVersion { private set; get; }
		private EVerifyLevel _verifyLevel;
		private string _defaultHostServer;
		private string _fallbackHostServer;

		/// <summary>
		/// 异步初始化
		/// </summary>
		public InitializationOperation InitializeAsync(bool clearCacheWhenDirty, bool ignoreResourceVersion,
			EVerifyLevel verifyLevel, string defaultHostServer, string fallbackHostServer)
		{
			ClearCacheWhenDirty = clearCacheWhenDirty;
			IgnoreResourceVersion = ignoreResourceVersion;
			_verifyLevel = verifyLevel;
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;

			var operation = new HostPlayModeInitializationOperation(this);
			OperationUpdater.ProcessOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 异步更新补丁清单
		/// </summary>
		public UpdateManifestOperation UpdatePatchManifestAsync(int updateResourceVersion, int timeout)
		{
			var operation = new HostPlayModeUpdateManifestOperation(this, updateResourceVersion, timeout);
			OperationUpdater.ProcessOperaiton(operation);
			return operation;
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (LocalPatchManifest == null)
				return 0;
			return LocalPatchManifest.ResourceVersion;
		}

		/// <summary>
		/// 获取内置资源标记列表
		/// </summary>
		public string[] GetManifestBuildinTags()
		{
			if (LocalPatchManifest == null)
				return new string[0];
			return LocalPatchManifest.GetBuildinTags();
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
			foreach (var patchBundle in LocalPatchManifest.BundleList)
			{
				// 忽略缓存资源
				if (Cache.Contains(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
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
			if (LocalPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
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
			if (LocalPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				MotionLog.Log($"Cache download web file : {patchBundle.BundleName} Version : {patchBundle.Version} Hash : {patchBundle.Hash}");
				Cache.CacheDownloadPatchFile(patchBundle.Hash);
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
			Cache.CacheDownloadPatchFiles(hashList);
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
		internal void ParseAndSaveRemotePatchManifest(string content)
		{
			LocalPatchManifest = PatchManifest.Deserialize(content);

			// 注意：这里会覆盖掉沙盒内的补丁清单文件
			MotionLog.Log("Save remote patch manifest file.");
			string savePath = AssetPathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
			PatchManifest.Serialize(savePath, LocalPatchManifest);
		}

		// WEB相关
		internal string GetPatchDownloadURL(int resourceVersion, string fileName)
		{
			if (IgnoreResourceVersion)
				return $"{_defaultHostServer}/{fileName}";
			else
				return $"{_defaultHostServer}/{resourceVersion}/{fileName}";
		}
		internal string GetPatchDownloadFallbackURL(int resourceVersion, string fileName)
		{
			if (IgnoreResourceVersion)
				return $"{_fallbackHostServer}/{fileName}";
			else
				return $"{_fallbackHostServer}/{resourceVersion}/{fileName}";
		}

		#region IBundleServices接口
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string bundleName)
		{
			if (string.IsNullOrEmpty(bundleName))
				return new AssetBundleInfo(string.Empty, string.Empty);

			if (LocalPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle patchBundle))
			{
				// 查询APP资源
				if (AppPatchManifest.Bundles.TryGetValue(bundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
					{
						string appLoadPath = AssetPathHelper.MakeStreamingLoadPath(appPatchBundle.Hash);
						AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, appLoadPath, appPatchBundle.Version, appPatchBundle.IsEncrypted, appPatchBundle.IsRawFile);
						return bundleInfo;
					}
				}

				// 查询沙盒资源
				string sandboxLoadPath = PatchHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
				if (Cache.Contains(patchBundle.Hash))
				{
					if (File.Exists(sandboxLoadPath))
					{
						AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, sandboxLoadPath, patchBundle.Version, patchBundle.IsEncrypted, patchBundle.IsRawFile);
						return bundleInfo;
					}
					else
					{
						MotionLog.Error($"Cache file is missing : {sandboxLoadPath}");
					}
				}

				// 从服务端下载
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
			return LocalPatchManifest.GetAssetBundleName(assetPath);
		}
		string[] IBundleServices.GetAllDependencies(string assetPath)
		{
			return LocalPatchManifest.GetAllDependencies(assetPath);
		}
		#endregion
	}
}