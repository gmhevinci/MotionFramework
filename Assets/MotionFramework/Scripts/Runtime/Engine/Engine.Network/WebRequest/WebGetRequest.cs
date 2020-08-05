//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace MotionFramework.Network
{
	public sealed class WebGetRequest : WebRequestBase
	{
		public WebGetRequest(string url) : base(url)
		{
		}
		public override void DownLoad()
		{
			if (CacheRequest != null)
				return;

			// 下载文件
			CacheRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
			DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
			CacheRequest.downloadHandler = handler;
			CacheRequest.disposeDownloadHandlerOnDispose = true;
			CacheRequest.timeout = Timeout;
			AsyncOperationHandle = CacheRequest.SendWebRequest();
		}
		public override void ReportError()
		{
			if(CacheRequest != null)
				MotionLog.Warning($"{nameof(WebGetRequest)} : {URL} Error : {CacheRequest.error}");
		}

		public byte[] GetData()
		{
			if (IsDone() && HasError() == false)
				return CacheRequest.downloadHandler.data;
			else
				return null;
		}
		public string GetText()
		{
			if (IsDone() && HasError() == false)
				return CacheRequest.downloadHandler.text;
			else
				return null;
		}
	}
}