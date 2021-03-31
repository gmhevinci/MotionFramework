//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace MotionFramework.Network
{
	public sealed class WebFileRequest : WebRequestBase
	{
		/// <summary>
		/// 文件存储路径
		/// </summary>
		public string SavePath { private set; get; }

		public WebFileRequest(string url, string savePath) : base(url)
		{
			SavePath = savePath;
		}
		public override void DownLoad()
		{
			if (CacheRequest != null)
				return;

			// 下载文件
			CacheRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
			DownloadHandlerFile handler = new DownloadHandlerFile(SavePath);
			handler.removeFileOnAbort = true;
			CacheRequest.downloadHandler = handler;
			CacheRequest.disposeDownloadHandlerOnDispose = true;
			AsyncOperationHandle = CacheRequest.SendWebRequest();
		}
		public override void ReportError()
		{
			if(CacheRequest != null)
				MotionLog.Warning($"{nameof(WebFileRequest)} : {URL} Error : {CacheRequest.error}");
		}
	}
}