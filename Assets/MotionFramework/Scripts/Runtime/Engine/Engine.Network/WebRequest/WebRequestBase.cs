//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace MotionFramework.Network
{
	/// <summary>
	/// 下载器基类
	/// 说明：UnityWebRequest(UWR) supports reading streaming assets since 2017.1
	/// </summary>
	public abstract class WebRequestBase
	{
		/// <summary>
		/// 下载路径
		/// </summary>
		public string URL { private set; get; }

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
		/// 下载状态
		/// </summary>
		public EWebRequestStates States { get; protected set; }

		/// <summary>
		/// 缓存的UnityWebRequest
		/// </summary>
		protected UnityWebRequest CacheRequest;


		public WebRequestBase(string url)
		{
			URL = url;
			States = EWebRequestStates.None;
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public abstract IEnumerator DownLoad();

		/// <summary>
		/// 释放下载器
		/// </summary>
		public void Dispose()
		{
			if(CacheRequest != null)
			{
				CacheRequest.Dispose();
				CacheRequest = null;
			}
		}

		/// <summary>
		/// 是否完毕（无论成功失败）
		/// </summary>
		public bool IsDone()
		{
			return States == EWebRequestStates.Success || States == EWebRequestStates.Fail;
		}
	}
}