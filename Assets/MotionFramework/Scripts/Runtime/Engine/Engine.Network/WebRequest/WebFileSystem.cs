//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Network
{
	public static class WebFileSystem
	{
		private static readonly Dictionary<string, WebFileRequest> _webFileRequestDic = new Dictionary<string, WebFileRequest>();
		private static readonly List<string> _removeWebFileList = new List<string>(100);


		/// <summary>
		/// 更新所有下载类
		/// </summary>
		internal static void Update()
		{
			_removeWebFileList.Clear();

			// 更新所有下载器
			foreach (var valuePair in _webFileRequestDic)
			{
				var reqeust = valuePair.Value;
				reqeust.Update();

				// 移除引用计数为零的下载类
				if (reqeust.RefCount <= 0)
				{
					_removeWebFileList.Add(valuePair.Key);
				}
			}

			// 移除引用计数为零的下载器
			foreach (var key in _removeWebFileList)
			{
				_webFileRequestDic.Remove(key);
			}
		}

		/// <summary>
		/// 获取文件下载类，如果不存在就创建新的下载类
		/// </summary>
		public static WebFileRequest GetWebFileRequest(string url, string savePath, int failedTryAgain, int timeout = 60)
		{
			if (_webFileRequestDic.TryGetValue(url, out var request))
			{
				request.RefCount++;
				return request;
			}
			else
			{
				var newRequest = new WebFileRequest(url);
				newRequest.RefCount++;
				newRequest.SendRequest(savePath, failedTryAgain, timeout);
				_webFileRequestDic.Add(url, newRequest);
				return newRequest;
			}
		}

		/// <summary>
		/// 获取当前请求的文件总数
		/// </summary>
		public static int GetRequestTotalCount()
		{
			return _webFileRequestDic.Count;
		}
	}
}