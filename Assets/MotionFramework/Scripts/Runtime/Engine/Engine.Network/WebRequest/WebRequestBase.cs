//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
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
	public abstract class WebRequestBase : IEnumerator
	{
		protected UnityWebRequest _webRequest;
		protected UnityWebRequestAsyncOperation _operationHandle;

		/// <summary>
		/// 请求URL地址
		/// </summary>
		public string URL { private set; get; }


		public WebRequestBase(string url)
		{
			URL = url;
		}

		/// <summary>
		/// 释放下载器
		/// </summary>
		public void Dispose()
		{
			if (_webRequest != null)
			{
				_webRequest.Dispose();
				_webRequest = null;
				_operationHandle = null;
			}
		}

		/// <summary>
		/// 是否完毕（无论成功失败）
		/// </summary>
		public bool IsDone()
		{
			if (_operationHandle == null)
				return false;
			return _operationHandle.isDone;
		}

		/// <summary>
		/// 下载是否发生错误
		/// </summary>
		public bool HasError()
		{
			if (_webRequest.isNetworkError || _webRequest.isHttpError)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 报告错误
		/// </summary>
		public void ReportError()
		{
			if (_webRequest != null)
			{
				MotionLog.Warning($"URL : {URL} Error : {_webRequest.error}");
			}
		}

		/// <summary>
		/// 获取错误信息
		/// </summary>
		public string GetError()
		{
			if (_webRequest != null)
			{
				return $"URL : {URL} Error : {_webRequest.error}";
			}
			return string.Empty;
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