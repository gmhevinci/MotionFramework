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
		private bool _isDispose = false;
		private bool _isDone = false;
		private string _savePath;
		private int _timeout;
		private int _failedTryAgain;

		// 下载超时相关
		// 注意：在连续时间段内无新增下载数据及判定为超时
		private float _latestDownloadRealtime = -1;
		private float _latestDownloadBytes = -1;

		/// <summary>
		/// 用户自定义数据类
		/// </summary>
		public object UserData { set; get; }

		/// <summary>
		/// 引用计数
		/// </summary>
		internal int RefCount = 0;

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


		internal WebFileRequest(string url) : base(url)
		{
		}
		internal void SendRequest(string savePath, int failedTryAgain, int timeout)
		{
			if (string.IsNullOrEmpty(savePath))
				throw new ArgumentNullException();

			if (_webRequest == null)
			{
				_savePath = savePath;
				_failedTryAgain = failedTryAgain;
				_timeout = timeout;

				_webRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
				DownloadHandlerFile handler = new DownloadHandlerFile(savePath);
				handler.removeFileOnAbort = true;
				_webRequest.downloadHandler = handler;
				_webRequest.disposeDownloadHandlerOnDispose = true;
				_operationHandle = _webRequest.SendWebRequest();
			}
		}
		internal void Update()
		{
			if (_webRequest == null)
				return;

			if (_isDone)
				return;

			if (_operationHandle.isDone)
			{
				// 如果发生错误，多尝试几次下载
				if(HasError() && _failedTryAgain > 0)
				{
					TryAgainRequest();
					return;
				}
				_isDone = true;
			}
			else
			{
				// 检测是否超时
				if (_latestDownloadBytes != DownloadedBytes)
				{
					_latestDownloadBytes = DownloadedBytes;
					_latestDownloadRealtime = Time.realtimeSinceStartup;
				}
				if ((Time.realtimeSinceStartup - _latestDownloadRealtime) > _timeout)
				{
					MotionLog.Warning($"Web file request timeout : {URL}");
					_webRequest.Abort();
					_isDone = true;
				}
			}
		}
		private void TryAgainRequest()
		{
			_failedTryAgain--;
			_latestDownloadRealtime = -1;
			_latestDownloadBytes = -1;

			// 注意：先清除旧数据，再创建新的下载器
			base.Dispose();
			SendRequest(_savePath, _failedTryAgain, _timeout);
			MotionLog.Warning($"Try again request : {URL}");
		}

		public override void Dispose()
		{
			if (_isDispose == false)
			{
				_isDispose = true;
				RefCount--;

				if (RefCount <= 0)
					base.Dispose();
			}
		}
		public override bool IsDone()
		{
			return _isDone;
		}
	}
}