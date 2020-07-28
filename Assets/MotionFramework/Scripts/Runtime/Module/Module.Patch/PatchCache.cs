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
		public const int DEFAULT_VERSION = -1;

		/// <summary>
		/// 缓存版本
		/// </summary>
		public string CacheVersion = string.Empty;

		/// <summary>
		/// 资源版本
		/// </summary>
		public int ResourceVersion = DEFAULT_VERSION;

		/// <summary>
		/// 缓存文件的哈希列表
		/// </summary>
		public List<string> CachedFileHashList = new List<string>();


		// 缓存操作方法
		public void CacheDownloadPatchFile(string hash)
		{
			if (CachedFileHashList.Contains(hash) == false)
			{
				CachedFileHashList.Add(hash);

				// 保存缓存
				SaveCache();
			}
		}
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
				// 保存缓存
				SaveCache();
			}
		}

		/// <summary>
		/// 缓存文件是否已经存在
		/// </summary>
		public bool Contains(string md5)
		{
			return CachedFileHashList.Contains(md5);
		}

		/// <summary>
		/// 保存缓存文件
		/// </summary>
		public void SaveCache()
		{
			MotionLog.Log("Save cache to disk.");
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = JsonUtility.ToJson(this);
			FileUtility.CreateFile(filePath, jsonData);
		}

		/// <summary>
		/// 清空缓存并删除所有缓存文件
		/// </summary>
		public void ClearCache()
		{
			MotionLog.Warning("Clear cache and remove all sandbox files.");
			CacheVersion = string.Empty;
			ResourceVersion = DEFAULT_VERSION;
			CachedFileHashList.Clear();
			PatchHelper.ClearSandbox();
		}

		/// <summary>
		/// 读取缓存文件
		/// </summary>
		public static PatchCache LoadCache()
		{
			if (PatchHelper.CheckSandboxCacheFileExist() == false)
				return new PatchCache();

			MotionLog.Log("Load cache from disk.");
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = FileUtility.ReadFile(filePath);
			return JsonUtility.FromJson<PatchCache>(jsonData);
		}
	}
}