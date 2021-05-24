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
		private bool _isDone = false;
		private bool _isError = false;

		// 保留参数
		private string _fallbackURL;
		private string _savePath;
		private int _timeout;
		private int _failedTryAgain;
		private int _requestCount;

		// 下载超时相关
		private bool _isAbort = false;
		private ulong _latestDownloadBytes;
		private float _latestDownloadRealtime;

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


		internal WebFileRequest(string mainURL, string fallbackURL) : base(mainURL)
		{
			_fallbackURL = fallbackURL;
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
				_requestCount++;

				// 重置超时相关变量
				_isAbort = false;
				_latestDownloadBytes = 0;
				_latestDownloadRealtime = Time.realtimeSinceStartup;

				_webRequest = new UnityWebRequest(GetRequestURL(), UnityWebRequest.kHttpVerbGET);
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
				if (IsError() && _failedTryAgain > 0)
				{
					TryAgainRequest();
				}
				else
				{
					_isError = IsError();
					_isDone = true;
				}
			}
			else
			{
				// 检测是否超时
				CheckTimeout();
			}
		}

		private string GetRequestURL()
		{
			// 轮流返回请求地址
			if (_requestCount % 2 == 0)
				return _fallbackURL;
			else
				return URL;
		}
		private void CheckTimeout()
		{
			// 注意：在连续时间段内无新增下载数据及判定为超时
			if (_isAbort == false)
			{
				if (_latestDownloadBytes != DownloadedBytes)
				{
					_latestDownloadBytes = DownloadedBytes;
					_latestDownloadRealtime = Time.realtimeSinceStartup;
				}

				float offset = Time.realtimeSinceStartup - _latestDownloadRealtime;
				if (offset > _timeout)
				{
					MotionLog.Warning($"Web file request timeout : {URL}");
					_webRequest.Abort();
					_isAbort = true;
				}
			}
		}
		private void TryAgainRequest()
		{
			_failedTryAgain--;

			// 报告错误
			ReportError();

			// 清除旧数据
			base.Dispose();

			// 重新请求下载
			MotionLog.Warning($"Try again request : {URL}");
			SendRequest(_savePath, _failedTryAgain, _timeout);
		}
		private bool IsError()
		{
			if (_webRequest.isNetworkError || _webRequest.isHttpError)
				return true;
			else
				return false;
		}

		public override void Dispose()
		{
			RefCount--;
			if (RefCount <= 0)
			{
				base.Dispose();
			}
		}
		public override bool IsDone()
		{
			return _isDone;
		}
		public override bool HasError()
		{
			return _isError;
		}
	}
}