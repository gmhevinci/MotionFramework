//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Patch
{
	public static class DownloadSystem
	{
		private static readonly Dictionary<string, FileDownloader> _downloaderDic = new Dictionary<string, FileDownloader>();
		private static readonly List<string> _removeList = new List<string>(100);


		/// <summary>
		/// 更新所有下载器
		/// </summary>
		internal static void Update()
		{
			_removeList.Clear();

			// 更新所有下载器
			foreach (var valuePair in _downloaderDic)
			{
				var reqeust = valuePair.Value;
				reqeust.Update();

				// 移除引用计数为零的下载器
				if (reqeust.RefCount <= 0)
				{
					_removeList.Add(valuePair.Key);
				}
			}

			// 移除引用计数为零的下载器
			foreach (var key in _removeList)
			{
				_downloaderDic.Remove(key);
			}
		}

		/// <summary>
		/// 获取文件下载器，如果不存在就创建新的下载器
		/// </summary>
		public static FileDownloader GetFileDownloader(string mainURL, string fallbackURL, string savePath, int failedTryAgain, int timeout = 60)
		{
			if (_downloaderDic.TryGetValue(mainURL, out var request))
			{
				request.Refrence();
				return request;
			}
			else
			{
				var newRequest = new FileDownloader(mainURL, fallbackURL);
				newRequest.Refrence();
				newRequest.SendRequest(savePath, failedTryAgain, timeout);
				_downloaderDic.Add(mainURL, newRequest);
				return newRequest;
			}
		}

		/// <summary>
		/// 获取下载器的总数
		/// </summary>
		public static int GetFileDownloaderTotalCount()
		{
			return _downloaderDic.Count;
		}
	}
}