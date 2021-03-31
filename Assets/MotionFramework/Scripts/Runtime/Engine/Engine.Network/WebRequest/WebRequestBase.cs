//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace MotionFramework.Network
{
	/// <summary>
	/// 下载器基类
	/// 说明：UnityWebRequest(UWR) supports reading streaming assets since 2017.1
	/// </summary>
	public abstract class WebRequestBase : IEnumerator
	{
		/// <summary>
		/// 缓存的UnityWebRequest
		/// </summary>
		protected UnityWebRequest CacheRequest;

		/// <summary>
		/// 异步操作句柄
		/// </summary>
		protected UnityWebRequestAsyncOperation AsyncOperationHandle;
	
		/// <summary>
		/// 下载路径
		/// </summary>
		public string URL { private set; get; }

		/// <summary>
		/// 下载超时, 默认60秒（单位：秒）
		/// 注意：在连续时间段内无新增下载数据及判定为超时
		/// </summary>
		public int Timeout { set; get; } = 60;

		/// <summary>
		/// 下载进度（0-100f）
		/// </summary>
		public float DownloadProgress
		{
			get
			{
				if (CacheRequest == null)
					return 0;
				return CacheRequest.downloadProgress * 100f;		
			}
		}

		/// <summary>
		/// 已经下载的字节数
		/// </summary>
		public ulong DownloadedBytes
		{
			get
			{
				if (CacheRequest == null)
					return 0;
				return CacheRequest.downloadedBytes;
			}
		}

		/// <summary>
		/// 用户数据
		/// </summary>
		public object UserData { set; get; }

		// 下载超时相关
		private float _latestDownloadRealtime = -1;
		private float _latestDownloadBytes = -1;


		public WebRequestBase(string url)
		{
			URL = url;
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public abstract void DownLoad();

		/// <summary>
		/// 报错错误
		/// </summary>
		public abstract void ReportError();

		/// <summary>
		/// 释放下载器
		/// </summary>
		public void Dispose()
		{
			if(CacheRequest != null)
			{
				CacheRequest.Dispose();
				CacheRequest = null;
				AsyncOperationHandle = null;
			}
		}

		/// <summary>
		/// 是否完毕（无论成功失败）
		/// </summary>
		/// <param name="checkTimeout">是否检测超时</param>
		public bool IsDone(bool checkTimeout = true)
		{
			if(checkTimeout)
				CheckTimeout();

			if (AsyncOperationHandle == null)
				return false;
			return AsyncOperationHandle.isDone;
		}

		/// <summary>
		/// 下载是否发送错误
		/// </summary>
		public bool HasError()
		{
			return CacheRequest.isNetworkError || CacheRequest.isHttpError;
		}

		/// <summary>
		/// 检测是否超时
		/// </summary>
		private void CheckTimeout()
		{
			if (CacheRequest == null || AsyncOperationHandle == null)
				return;

			if(_latestDownloadBytes != DownloadedBytes)
			{
				_latestDownloadBytes = DownloadedBytes;
				_latestDownloadRealtime = Time.realtimeSinceStartup;
			}
			if ((Time.realtimeSinceStartup - _latestDownloadRealtime) > Timeout)
			{
				MotionLog.Warning($"Web request timeout : {URL}");
				CacheRequest.Abort();
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