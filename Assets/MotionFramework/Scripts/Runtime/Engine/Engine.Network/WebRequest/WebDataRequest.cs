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
	public class WebDataRequest : WebRequestBase
	{
		public WebDataRequest(string url) : base(url)
		{
		}
		public override IEnumerator DownLoad()
		{
			// Check fatal
			if (States != EWebRequestStates.None)
				throw new Exception($"{nameof(WebDataRequest)} is downloading yet : {URL}");

			States = EWebRequestStates.Loading;

			// 下载文件
			CacheRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
			DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
			CacheRequest.downloadHandler = handler;
			CacheRequest.disposeDownloadHandlerOnDispose = true;
			CacheRequest.timeout = NetworkDefine.WebRequestTimeout;
			yield return CacheRequest.SendWebRequest();

			// Check error
			if (CacheRequest.isNetworkError || CacheRequest.isHttpError)
			{
				MotionLog.Log(ELogLevel.Warning, $"Failed to download web data : {URL} Error : {CacheRequest.error}");
				States = EWebRequestStates.Fail;
			}
			else
			{
				States = EWebRequestStates.Success;
			}
		}

		public byte[] GetData()
		{
			if (States == EWebRequestStates.Success)
				return CacheRequest.downloadHandler.data;
			else
				return null;
		}
		public string GetText()
		{
			if (States == EWebRequestStates.Success)
				return CacheRequest.downloadHandler.text;
			else
				return null;
		}
	}
}