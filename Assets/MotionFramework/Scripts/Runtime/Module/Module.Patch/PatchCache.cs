//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Resource;
using MotionFramework.Utility;

namespace MotionFramework.Patch
{
	[Serializable]
	public class PatchCache
	{
		/// <summary>
		/// 缓存的APP内置版本
		/// </summary>
		public string CacheAppVersion = string.Empty;

		/// <summary>
		/// 缓存文件的哈希列表
		/// </summary>
		public List<string> CachedFileHashList = new List<string>();


		/// <summary>
		/// 初始化APP版本号
		/// </summary>
		/// <param name="appVersion"></param>
		public void InitAppVersion(string appVersion)
		{
			CacheAppVersion = appVersion;
			SaveCache();
		}

		/// <summary>
		/// 清理缓存
		/// </summary>
		public void ClearCache()
		{
			MotionLog.Warning("Clear cache and delete cache folder.");
			CacheAppVersion = string.Empty;
			CachedFileHashList.Clear();
			PatchHelper.DeleteSandboxCacheFolder();
		}

		/// <summary>
		/// 缓存文件是否已经存在
		/// </summary>
		public bool Contains(string hash)
		{
			return CachedFileHashList.Contains(hash);
		}

		/// <summary>
		/// 缓存单个文件
		/// </summary>
		public void CacheDownloadPatchFile(string hash)
		{
			if (CachedFileHashList.Contains(hash) == false)
			{
				CachedFileHashList.Add(hash);
				SaveCache();
			}
		}

		/// <summary>
		/// 缓存多个文件
		/// </summary>
		public void CacheDownloadPatchFiles(List<string> hashList)
		{
			bool hasCached = false;
			foreach (var hash in hashList)
			{
				if (CachedFileHashList.Contains(hash) == false)
				{
					CachedFileHashList.Add(hash);
					hasCached = true;
				}
			}

			if (hasCached)
			{
				SaveCache();
			}
		}

		/// <summary>
		/// 保存缓存文件
		/// </summary>
		private void SaveCache()
		{
			MotionLog.Log("Save patch cache to disk.");
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = JsonUtility.ToJson(this);
			FileUtility.CreateFile(filePath, jsonData);
		}

		/// <summary>
		/// 读取缓存文件
		/// </summary>
		public static PatchCache LoadCache()
		{
			MotionLog.Log("Load cache from disk.");
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = FileUtility.ReadFile(filePath);
			return JsonUtility.FromJson<PatchCache>(jsonData);
		}
	}
}