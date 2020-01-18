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
	public class WebFileRequest : WebRequestBase
	{
		/// <summary>
		/// 文件存储路径
		/// </summary>
		public string SavePath { private set; get; }

		public WebFileRequest(string url, string savePath) : base(url)
		{
			SavePath = savePath;
		}
		public override IEnumerator DownLoad()
		{
			// Check fatal
			if (States != EWebRequestStates.None)
				throw new Exception($"{nameof(WebFileRequest)} is downloading yet : {URL}");

			States = EWebRequestStates.Loading;

			// 下载文件
			CacheRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
			DownloadHandlerFile handler = new DownloadHandlerFile(SavePath);
			handler.removeFileOnAbort = true;
			CacheRequest.downloadHandler = handler;
			CacheRequest.disposeDownloadHandlerOnDispose = true;
			CacheRequest.timeout = NetworkDefine.WebRequestTimeout;
			yield return CacheRequest.SendWebRequest();

			// Check error
			if (CacheRequest.isNetworkError || CacheRequest.isHttpError)
			{
				MotionLog.Log(ELogLevel.Warning, $"Failed to download web file : {URL} Error : {CacheRequest.error}");
				States = EWebRequestStates.Fail;
			}
			else
			{
				States = EWebRequestStates.Success;
			}
		}
	}
}