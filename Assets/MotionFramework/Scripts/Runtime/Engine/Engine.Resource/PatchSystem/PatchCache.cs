//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
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
		/// 初始化APP版本号
		/// </summary>
		/// <param name="appVersion">应用程序版本</param>
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
			PatchHelper.DeleteSandboxCacheFolder();
		}

		/// <summary>
		/// 保存缓存文件
		/// </summary>
		private void SaveCache()
		{
			MotionLog.Log("Save application version to disk.");
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