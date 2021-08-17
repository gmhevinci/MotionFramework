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

namespace MotionFramework.Patch
{
	internal sealed class FileDownloader : IEnumerator
	{
		private UnityWebRequest _webRequest;
		private UnityWebRequestAsyncOperation _operationHandle;

		private bool _isDone = false;
		private bool _isError = false;

		// 保留参数
		private string _fallbackURL;
		private string _savePath;
		private int _timeout;
		private int _failedTryAgain;
		private int _requestCount;
		private string _requestURL;

		// 下载超时相关
		private bool _isAbort = false;
		private ulong _latestDownloadBytes;
		private float _latestDownloadRealtime;

		/// <summary>
		/// 请求URL地址
		/// </summary>
		public string URL { private set; get; }

		/// <summary>
		/// 用户自定义数据类
		/// </summary>
		public object UserData { set; get; }

		/// <summary>
		/// 引用计数
		/// </summary>
		internal int RefCount { private set; get; } = 0;

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


		internal FileDownloader(string mainURL, string fallbackURL)
		{
			URL = mainURL;
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
				_requestURL = GetRequestURL();

				// 重置超时相关变量
				_isAbort = false;
				_latestDownloadBytes = 0;
				_latestDownloadRealtime = Time.realtimeSinceStartup;

				_webRequest = new UnityWebRequest(_requestURL, UnityWebRequest.kHttpVerbGET);
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
				if (CheckError() && _failedTryAgain > 0)
				{
					TryAgainRequest();
				}
				else
				{
					_isError = CheckError();
					_isDone = true;
				}
			}
			else
			{
				// 检测是否超时
				CheckTimeout();
			}
		}
		internal void Refrence()
		{
			RefCount++;
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
					MotionLog.Warning($"Web file request timeout : {_requestURL}");
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

			// 释放请求句柄
			DisposeWebRequest();

			// 重新请求下载
			SendRequest(_savePath, _failedTryAgain, _timeout);
			MotionLog.Warning($"Try again request : {_requestURL}");
		}
		private bool CheckError()
		{
			if (_webRequest.isNetworkError || _webRequest.isHttpError)
				return true;
			else
				return false;
		}
		private void DisposeWebRequest()
		{
			if (_webRequest != null)
			{
				_webRequest.Dispose();
				_webRequest = null;
				_operationHandle = null;
			}
		}

		/// <summary>
		/// 释放下载器
		/// </summary>
		public void Dispose()
		{
			RefCount--;
			if (RefCount <= 0)
			{
				DisposeWebRequest();
			}
		}

		/// <summary>
		/// 检测下载器是否已经完成（无论成功或失败）
		/// </summary>
		public bool IsDone()
		{
			return _isDone;
		}

		/// <summary>
		/// 下载过程是否发生错误
		/// </summary>
		/// <returns></returns>
		public bool HasError()
		{
			return _isError;
		}

		/// <summary>
		/// 报告下载时发生的错误
		/// </summary>
		public void ReportError()
		{
			if (_webRequest != null)
			{
				MotionLog.Warning($"URL : {_requestURL} Error : {_webRequest.error}");
			}
		}

		#region 异步相关
		bool IEnumerator.MoveNext()
		{
			return !IsDone();
		}
		void IEnumerator.Reset()
		{
		}
		object IEnumerator.Current
		{
			get { return null; }
		}
		#endregion
	}
}