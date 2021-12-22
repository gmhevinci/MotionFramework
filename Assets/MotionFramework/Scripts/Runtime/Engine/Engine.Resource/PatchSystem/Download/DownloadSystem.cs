//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Utility;

namespace MotionFramework.Resource
{
	internal static class DownloadSystem
    {
		public static PatchCache Cache;

		private static readonly Dictionary<string, FileDownloader> _downloaderDic = new Dictionary<string, FileDownloader>();
		private static readonly List<string> _removeList = new List<string>(100);
		private static readonly Timer _saveTimer = Timer.CreateOnceTimer(3f);
		private static bool _firstRun = true;

		/// <summary>
		/// 更新所有下载器
		/// </summary>
		public static void Update()
		{
			if(_firstRun)
			{
				_firstRun = false;
				_saveTimer.Kill();
			}

			// 更新下载器
			_removeList.Clear();
			foreach (var valuePair in _downloaderDic)
			{
				var downloader = valuePair.Value;
				downloader.Update();
				if (downloader.IsDone())
					_removeList.Add(valuePair.Key);
			}

			// 移除下载器
			foreach (var key in _removeList)
			{
				_downloaderDic.Remove(key);
			}

			// 自动保存存储文件
			if(_saveTimer.Update(UnityEngine.Time.unscaledDeltaTime))
			{
				Cache.SaveCache();
			}
		}

		/// <summary>
		/// 开始下载资源文件
		/// 注意：只有第一次请求的参数才是有效的
		/// </summary>
		public static FileDownloader BeginDownload(AssetBundleInfo bundleInfo, int failedTryAgain, int timeout = 60)
		{
			MotionLog.Log($"Beginning to download file : {bundleInfo.BundleName} URL : {bundleInfo.RemoteMainURL}");

			if (_downloaderDic.TryGetValue(bundleInfo.Hash, out var downloader))
			{
				return downloader;
			}
			
			// 创建新的下载器	
			{
				FileUtility.CreateFileDirectory(bundleInfo.LocalPath);			
				var newDownloader = new FileDownloader(bundleInfo);
				newDownloader.SendRequest(failedTryAgain, timeout);
				_downloaderDic.Add(bundleInfo.Hash, newDownloader);
				return newDownloader;
			}
		}

		/// <summary>
		/// 获取下载器的总数
		/// </summary>
		public static int GetDownloaderTotalCount()
		{
			return _downloaderDic.Count;
		}


		/// <summary>
		/// 缓存单个文件
		/// </summary>
		public static void CacheDownloadPatchFile(AssetBundleInfo bundleInfo)
		{
			MotionLog.Log($"Cache download web file : {bundleInfo.BundleName} Version : {bundleInfo.Version} Hash : {bundleInfo.Hash}");
			Cache.CacheDownloadPatchFile(bundleInfo.Hash, false);
			_saveTimer.Reset();
		}

		/// <summary>
		/// 缓存多个文件
		/// </summary>
		public static void CacheDownloadPatchFiles(List<PatchBundle> downloadList)
		{
			List<string> hashList = new List<string>(downloadList.Count);
			foreach (var patchBundle in downloadList)
			{
				MotionLog.Log($"Cache download web file : {patchBundle.BundleName} Version : {patchBundle.Version} Hash : {patchBundle.Hash}");
				hashList.Add(patchBundle.Hash);
			}
			Cache.CacheDownloadPatchFiles(hashList, true);
		}

		/// <summary>
		/// 验证文件完整性
		/// </summary>
		public static bool CheckContentIntegrity(string filePath, string crc, long size)
		{
			if (File.Exists(filePath) == false)
				return false;

			// 先验证文件大小
			long fileSize = FileUtility.GetFileSize(filePath);
			if (fileSize != size)
				return false;

			// 再验证文件CRC
			string fileCRC = HashUtility.FileCRC32(filePath);
			return fileCRC == crc;
		}
	}
}