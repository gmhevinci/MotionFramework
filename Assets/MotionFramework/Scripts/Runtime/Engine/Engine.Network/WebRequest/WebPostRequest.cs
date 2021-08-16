//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace MotionFramework.Network
{
	public sealed class WebPostRequest : WebRequestBase
	{
		public WebPostRequest(string url) : base(url)
		{
		}

		/// <summary>
		/// 发送POST请求
		/// </summary>
		/// <param name="post">POST的文本内容</param>
		/// <param name="timeout">超时：从请求开始计时</param>
		public void SendRequest(string post, int timeout = 0)
		{
			// Check error
			if (string.IsNullOrEmpty(post))
				throw new Exception($"Web post content is null or empty : {URL}");

			if (_webRequest == null)
			{
				_webRequest = UnityWebRequest.Post(URL, post);
				SendRequestInternal(timeout);
			}
		}

		/// <summary>
		/// 发送POST请求
		/// </summary>
		/// <param name="form">POST的表单数据</param>
		/// <param name="timeout">超时：从请求开始计时</param>
		public void SendRequest(WWWForm form, int timeout = 0)
		{
			// Check error
			if (form == null)
				throw new Exception($"Web post content is null or empty : {URL}");

			if (_webRequest == null)
			{
				_webRequest = UnityWebRequest.Post(URL, form);
				SendRequestInternal(timeout);
			}
		}

		/// <summary>
		/// 获取响应的文本数据
		/// </summary>
		public string GetResponse()
		{
			if (_webRequest != null && IsDone())
				return _webRequest.downloadHandler.text;
			else
				return null;
		}

		private void SendRequestInternal(int timeout)
		{
			DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
			_webRequest.downloadHandler = handler;
			_webRequest.disposeDownloadHandlerOnDispose = true;
			_webRequest.timeout = timeout;
			_operationHandle = _webRequest.SendWebRequest();
		}
	}
}