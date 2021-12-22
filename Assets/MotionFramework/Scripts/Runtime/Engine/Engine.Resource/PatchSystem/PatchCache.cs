//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MotionFramework.Utility;

namespace MotionFramework.Resource
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
		/// 修复缓存
		/// 注意：在沙盒内文件被意外删除的时候，自我修复缓存列表
		/// </summary>
		public void RepairCache()
		{
			bool isChange = false;
			for (int i = CachedFileHashList.Count - 1; i >= 0; i--)
			{
				string fileHash = CachedFileHashList[i];
				string filePath = PatchHelper.MakeSandboxCacheFilePath(fileHash);
				if (File.Exists(filePath) == false)
				{
					MotionLog.Error($"Cache file is missing : {fileHash}");
					CachedFileHashList.RemoveAt(i);
					isChange = true;
				}
			}
			if (isChange)
			{
				SaveCache();
			}
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
		public void CacheDownloadPatchFile(string hash, bool autoSave)
		{
			if (CachedFileHashList.Contains(hash) == false)
			{
				CachedFileHashList.Add(hash);
				if (autoSave)
				{
					SaveCache();
				}
			}
		}

		/// <summary>
		/// 缓存多个文件
		/// </summary>
		public void CacheDownloadPatchFiles(List<string> hashList, bool autoSave)
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

			if (hasCached && autoSave)
			{
				SaveCache();
			}
		}

		/// <summary>
		/// 保存缓存文件
		/// </summary>
		public void SaveCache()
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