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
	public class WebPostRequest : WebRequestBase
	{
		public string PostData { private set; get; }

		public WebPostRequest(string url, string post) : base(url)
		{
			PostData = post;
		}
		public override IEnumerator DownLoad()
		{
			// Check fatal
			if (string.IsNullOrEmpty(PostData))
				throw new Exception($"{nameof(WebPostRequest)} post content is null or empty : {URL}");

			// Check fatal
			if (States != EWebRequestStates.None)
				throw new Exception($"{nameof(WebPostRequest)} is downloading yet : {URL}");

			States = EWebRequestStates.Loading;

			// 下载文件
			CacheRequest = UnityWebRequest.Post(URL, PostData);
			DownloadHandlerBuffer downloadhandler = new DownloadHandlerBuffer();
			CacheRequest.downloadHandler = downloadhandler;
			CacheRequest.disposeDownloadHandlerOnDispose = true;
			CacheRequest.timeout = NetworkDefine.WebRequestTimeout;
			yield return CacheRequest.SendWebRequest();

			// Check error
			if (CacheRequest.isNetworkError || CacheRequest.isHttpError)
			{
				MotionLog.Log(ELogLevel.Warning, $"Failed to request web post : {URL} Error : {CacheRequest.error}");
				States = EWebRequestStates.Fail;
			}
			else
			{
				States = EWebRequestStates.Success;
			}
		}

		public string GetResponse()
		{
			if (States == EWebRequestStates.Success)
				return CacheRequest.downloadHandler.text;
			else
				return null;
		}
	}
}