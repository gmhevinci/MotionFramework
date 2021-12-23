//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using MotionFramework.Utility;

namespace MotionFramework.Resource
{
	internal sealed class FileDownloader
	{
		public AssetBundleInfo BundleInfo { private set; get; }
		private UnityWebRequest _webRequest;
		private UnityWebRequestAsyncOperation _operationHandle;

		private bool _isDone = false;
		private bool _isError = false;
		private string _lastError = string.Empty;

		// 保留参数
		private int _timeout;
		private int _failedTryAgain;
		private int _requestCount;
		private string _requestURL;

		// 下载超时相关
		private bool _isAbort = false;
		private ulong _latestDownloadBytes;
		private float _latestDownloadRealtime;

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

		private bool _waitTryAgain = false;
		private Timer _waitTimer = Timer.CreateOnceTimer(0.5f);

		internal FileDownloader(AssetBundleInfo bundleInfo)
		{
			BundleInfo = bundleInfo;
		}
		internal void SendRequest(int failedTryAgain, int timeout)
		{
			if (string.IsNullOrEmpty(BundleInfo.LocalPath))
				throw new ArgumentNullException();

			if (_webRequest == null)
			{
				_failedTryAgain = failedTryAgain;
				_timeout = timeout;
				_requestCount++;
				_requestURL = GetRequestURL();

				// 重置超时相关变量
				_isAbort = false;
				_latestDownloadBytes = 0;
				_latestDownloadRealtime = Time.realtimeSinceStartup;

				_webRequest = new UnityWebRequest(_requestURL, UnityWebRequest.kHttpVerbGET);
				DownloadHandlerFile handler = new DownloadHandlerFile(BundleInfo.LocalPath);
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

			// 等待再次执行
			if (_waitTryAgain)
			{
				if (_waitTimer.Update(Time.unscaledDeltaTime))
				{
					_waitTryAgain = false;
					TryAgainRequest();
				}
				return;
			}

			if (_operationHandle.isDone)
			{
				// 如果还有机会重新再来一次
				if (_failedTryAgain > 0)
				{
					if (CheckDownloadError())
					{
						_waitTryAgain = true;
						_waitTimer.Reset();
					}
					else
					{
						_isDone = true;
						_isError = false;
					}
				}
				else
				{
					_isDone = true;
					_isError = CheckDownloadError();
				}

				if (_isDone)
				{
					if (_isError == false)
					{
						DownloadSystem.CacheVerifyFile(BundleInfo.Hash, BundleInfo.BundleName);
					}
					else
					{
						// 注意：如果文件验证失败需要删除文件
						if (File.Exists(BundleInfo.LocalPath))
							File.Delete(BundleInfo.LocalPath);
					}

					// 释放下载请求
					DisposeWebRequest();
				}
			}
			else
			{
				// 检测是否超时
				CheckTimeout();
			}
		}
		internal void SetDone()
		{
			_isDone = true;
		}

		private string GetRequestURL()
		{
			// 轮流返回请求地址
			if (_requestCount % 2 == 0)
				return BundleInfo.RemoteFallbackURL;
			else
				return BundleInfo.RemoteMainURL;
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
			SendRequest(_failedTryAgain, _timeout);
			MotionLog.Warning($"Try again request : {_requestURL}");
		}
		private bool CheckDownloadError()
		{
			if (_webRequest.isNetworkError || _webRequest.isHttpError)
			{
				_lastError = _webRequest.error;
				return true;
			}
			else
			{
				// 注意：如果网络没有错误需要检测文件完整性
				if (DownloadSystem.CheckContentIntegrity(BundleInfo))
				{
					return false;
				}
				else
				{
					_lastError = $"Verify file content failed : {BundleInfo.BundleName}";
					return true;
				}
			}
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
		/// 报告错误信息
		/// </summary>
		public void ReportError()
		{
			MotionLog.Error($"URL : {_requestURL} Error : {_lastError}");
		}
	}
}