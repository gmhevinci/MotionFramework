//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MotionFramework.Network
{
	public sealed class WebFileRequest : WebRequestBase
	{
		/// <summary>
		/// 下载进度（0-100f）
		/// </summary>
		public float DownloadProgress
		{
			get
			{
				if (_webRequest == null)
					return 0;
				return _webRequest.downloadProgress * 100f;
			}
		}

		/// <summary>
		/// 已经下载的总字节数
		/// </summary>
		public ulong DownloadedBytes
		{
			get
			{
				if (_webRequest == null)
					return 0;
				return _webRequest.downloadedBytes;
			}
		}


		public WebFileRequest(string url) : base(url)
		{
		}

		/// <summary>
		/// 发送下载文件请求
		/// </summary>
		/// <param name="savePath">下载文件的保存路径</param>
		/// <param name="timeout">超时：从请求开始计时</param>
		public void SendRequest(string savePath, int timeout = 0)
		{
			if (string.IsNullOrEmpty(savePath))
				throw new ArgumentNullException();

			if (_webRequest == null)
			{
				_webRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
				DownloadHandlerFile handler = new DownloadHandlerFile(savePath);
				handler.removeFileOnAbort = true;
				_webRequest.timeout = timeout;
				_webRequest.downloadHandler = handler;
				_webRequest.disposeDownloadHandlerOnDispose = true;
				_operationHandle = _webRequest.SendWebRequest();
			}
		}
	}
}