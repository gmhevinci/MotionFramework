//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using MotionFramework.Utility;
using MotionFramework.Resource;
using MotionFramework.Network;

namespace MotionFramework.Patch
{
	internal class PatchCache
	{
		[Serializable]
		public class CacheData
		{
			/// <summary>
			/// 记录版本
			/// </summary>
			public string CacheVersion = string.Empty;

			/// <summary>
			/// 缓存文件的哈希列表
			/// </summary>
			public List<string> CachedFileHashList = new List<string>();
		}

		private readonly PatchManagerImpl _patcher;
		private readonly ECheckLevel _checkLevel;
		private PatchManifest _appPatchManifest;
		private PatchManifest _localPatchManifest;
		private PatchManifest _remotePatchManifest;
		private CacheData _cacheData;

		/// <summary>
		/// 本地资源版本号
		/// </summary>
		public int LocalResourceVersion
		{
			get
			{
				if (_localPatchManifest == null)
					return 0;
				return _localPatchManifest.ResourceVersion;
			}
		}


		public PatchCache(PatchManagerImpl patcher, ECheckLevel checkLevel)
		{
			_patcher = patcher;
			_checkLevel = checkLevel;
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAsync()
		{
			MotionLog.Log($"Beginning to initialize cache");

			// 加载缓存
			_cacheData = LoadCache();

			// 检测沙盒被污染
			CheckSandboxDirty();

			// 加载APP内的补丁清单
			MotionLog.Log($"Load app patch file.");
			{
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

				string jsonData = downloader.GetText();
				_appPatchManifest = PatchManifest.Deserialize(jsonData);
				downloader.Dispose();
			}

			// 加载沙盒内的补丁清单
			MotionLog.Log($"Load sandbox patch file.");
			{
				if (PatchHelper.CheckSandboxPatchManifestFileExist())
				{
					string filePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestFileName);
					string jsonData = File.ReadAllText(filePath);
					_localPatchManifest = PatchManifest.Deserialize(jsonData);
				}
				else
				{
					_localPatchManifest = _appPatchManifest;
				}
			}
		}

		/// <summary>
		/// 检测沙盒被污染
		/// 注意：在覆盖安装的时候，会保留沙盒目录里的文件，所以需要强制清空
		/// </summary>
		private void CheckSandboxDirty()
		{
			// 如果是首次打开，记录APP版本号
			if (PatchHelper.CheckSandboxCacheFileExist() == false)
			{
				_cacheData.CacheVersion = Application.version;
				SaveCache();
				return;
			}

			// 每次启动时比对APP版本号是否一致	
			if (_cacheData.CacheVersion != Application.version)
			{
				MotionLog.Warning($"Cache is dirty ! Cache version is {_cacheData.CacheVersion}, APP version is {Application.version}");
				ClearCache();

				// 重新写入最新的APP版本号
				_cacheData.CacheVersion = Application.version;
				SaveCache();
			}
		}

		/// <summary>
		/// 清空缓存并删除所有沙盒文件
		/// </summary>
		public void ClearCache()
		{
			MotionLog.Warning("Clear cache and remove all sandbox files.");
			PatchHelper.ClearSandbox();

			_appPatchManifest = null;
			_localPatchManifest = null;
			_remotePatchManifest = null;
			_cacheData = null;
		}

		/// <summary>
		/// 获取本地补丁清单
		/// </summary>
		public PatchManifest GetPatchManifest()
		{
			return _localPatchManifest;
		}

		/// <summary>
		/// 获取下载清单
		/// </summary>
		public List<PatchElement> GetDownloadList()
		{
			List<PatchElement> downloadList = new List<PatchElement>(1000);

			// 准备下载列表
			foreach (var webElement in _remotePatchManifest.ElementList)
			{
				// 忽略DLC资源
				if (webElement.IsDLC())
					continue;

				// 内置资源比较
				if (_appPatchManifest.Elements.TryGetValue(webElement.Name, out PatchElement appElement))
				{
					if (appElement.IsDLC() == false && appElement.MD5 == webElement.MD5)
						continue;
				}

				// 缓存资源比较
				if (_cacheData.CachedFileHashList.Contains(webElement.MD5))
					continue;

				downloadList.Add(webElement);
			}

			// 检测文件是否已经下载完毕
			// 注意：如果玩家在加载过程中强制退出，下次再进入的时候跳过已经加载的文件
			for (int i = downloadList.Count - 1; i >= 0; i--)
			{
				var element = downloadList[i];
				if (CheckPatchFileValid(element))
					downloadList.RemoveAt(i);
			}

			return downloadList;
		}

		/// <summary>
		/// 当远端补丁清单下载完毕
		/// </summary>
		public void OnDownloadRemotePatchManifest(string content)
		{
			if (_remotePatchManifest != null)
				throw new Exception("Should never get here.");
			_remotePatchManifest = PatchManifest.Deserialize(content);
		}

		/// <summary>
		/// 当远端补丁文件下载完毕
		/// </summary>
		public void OnDownloadRemotePatchFile(List<PatchElement> downloadList)
		{
			// 把下载文件得哈希值写入到缓存
			foreach (var element in downloadList)
			{
				if (_cacheData.CachedFileHashList.Contains(element.MD5) == false)
					_cacheData.CachedFileHashList.Add(element.MD5);
			}

			// 保存缓存
			SaveCache();

			// 保存补丁清单
			// 注意：这里会覆盖掉沙盒内的补丁清单文件
			_localPatchManifest = _remotePatchManifest;
			string savePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestFileName);
			PatchManifest.Serialize(savePath, _remotePatchManifest);
		}

		/// <summary>
		/// 缓存下载的远端补丁文件
		/// </summary>
		public void CacheDownloadPatchFile(AssetBundleInfo bundleInfo)
		{
			// 把下载文件得哈希值写入到缓存
			if (_cacheData.CachedFileHashList.Contains(bundleInfo.MD5) == false)
				_cacheData.CachedFileHashList.Add(bundleInfo.MD5);

			// 保存缓存
			SaveCache();
		}
		
		/// <summary>
		/// 获取AssetBundle加载信息
		/// </summary>
		public AssetBundleInfo GetAssetBundleLoadInfo(string manifestPath)
		{
			if (_localPatchManifest.Elements.TryGetValue(manifestPath, out PatchElement element))
			{
				// 查询内置资源
				if (_appPatchManifest.Elements.TryGetValue(manifestPath, out PatchElement appElement))
				{
					if (appElement.IsDLC() == false && appElement.MD5 == element.MD5)
					{
						string appLoadPath = AssetPathHelper.MakeStreamingLoadPath(manifestPath);
						AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, appLoadPath, string.Empty, appElement.MD5, appElement.SizeBytes, appElement.Version, appElement.IsEncrypted);
						return bundleInfo;
					}
				}

				// 查询缓存资源
				// 注意：如果沙盒内缓存文件不存在，那么将会从服务器下载
				string sandboxLoadPath = PatchHelper.MakeSandboxCacheFilePath(element.MD5);
				if (_cacheData.CachedFileHashList.Contains(element.MD5))
				{
					AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, sandboxLoadPath, string.Empty, element.MD5, element.SizeBytes, element.Version, element.IsEncrypted);
					return bundleInfo;
				}
				else
				{
					string remoteURL = _patcher.GetWebDownloadURL(element.Version.ToString(), element.Name);
					AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, sandboxLoadPath, remoteURL, element.MD5, element.SizeBytes, element.Version, element.IsEncrypted);
					return bundleInfo;
				}
			}
			else
			{
				MotionLog.Warning($"Not found element in patch manifest : {manifestPath}");
				AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, string.Empty);
				return bundleInfo;
			}
		}

		/// <summary>
		/// 检测补丁文件有效性
		/// </summary>
		public bool CheckPatchFileValid(PatchElement element)
		{
			string filePath = PatchHelper.MakeSandboxCacheFilePath(element.MD5);
			if (File.Exists(filePath) == false)
				return false;

			// 校验沙盒里的补丁文件
			if (_checkLevel == ECheckLevel.CheckSize)
			{
				long fileSize = FileUtility.GetFileSize(filePath);
				if (fileSize == element.SizeBytes)
					return true;
			}
			else if (_checkLevel == ECheckLevel.CheckMD5)
			{
				string md5 = HashUtility.FileMD5(filePath);
				if (md5 == element.MD5)
					return true;
			}
			else
			{
				throw new NotImplementedException(_checkLevel.ToString());
			}
			return false;
		}

		/// <summary>
		/// 读取缓存文件
		/// </summary>
		private CacheData LoadCache()
		{
			if (PatchHelper.CheckSandboxCacheFileExist() == false)
				return new CacheData();

			MotionLog.Log("Load cache file.");
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = FileUtility.ReadFile(filePath);
			return JsonUtility.FromJson<CacheData>(jsonData);
		}
		
		/// <summary>
		/// 保存缓存文件
		/// </summary>
		public void SaveCache()
		{
			if (_cacheData == null)
				throw new Exception($"{nameof(CacheData)} is null.");

			MotionLog.Log("Save cache file.");
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = JsonUtility.ToJson(_cacheData);
			FileUtility.CreateFile(filePath, jsonData);
		}
	}
}