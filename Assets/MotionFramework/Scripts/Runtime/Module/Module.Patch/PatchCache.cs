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
	public class CacheData
	{
		/// <summary>
		/// 缓存版本
		/// </summary>
		public string CacheVersion = string.Empty;

		/// <summary>
		/// 缓存文件的哈希列表
		/// </summary>
		public List<string> CachedFileHashList = new List<string>();

		// 缓存操作方法
		public void CacheDownloadPatchFile(AssetBundleInfo bundleInfo)
		{
			if (CachedFileHashList.Contains(bundleInfo.MD5) == false)
			{
				CachedFileHashList.Add(bundleInfo.MD5);

				// 保存缓存
				SaveCache();
			}
		}
		public void CacheDownloadPatchFile(PatchElement element)
		{
			if (CachedFileHashList.Contains(element.MD5) == false)
			{
				CachedFileHashList.Add(element.MD5);

				// 保存缓存
				SaveCache();
			}
		}
		public void CacheDownloadPatchFiles(List<PatchElement> elements)
		{
			bool hasCached = false;
			foreach (var element in elements)
			{
				if (CachedFileHashList.Contains(element.MD5) == false)
				{
					CachedFileHashList.Add(element.MD5);
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
			MotionLog.Log("Save patch cache file.");
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = JsonUtility.ToJson(this);
			FileUtility.CreateFile(filePath, jsonData);
		}

		/// <summary>
		/// 读取缓存文件
		/// </summary>
		public static CacheData LoadCache()
		{
			if (PatchHelper.CheckSandboxCacheFileExist() == false)
				return new CacheData();

			MotionLog.Log("Load patch cache file.");
			string filePath = PatchHelper.GetSandboxCacheFilePath();
			string jsonData = FileUtility.ReadFile(filePath);
			return JsonUtility.FromJson<CacheData>(jsonData);
		}
	}
}