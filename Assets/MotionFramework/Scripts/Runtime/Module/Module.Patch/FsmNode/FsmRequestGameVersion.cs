//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.AI;
using MotionFramework.Network;

namespace MotionFramework.Patch
{
	internal class FsmRequestGameVersion : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmRequestGameVersion(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.RequestGameVersion.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.RequestGameVersion);
			MotionEngine.StartCoroutine(Download());
		}
		void IFsmNode.OnUpdate()
		{
		}
		void IFsmNode.OnExit()
		{
		}
		void IFsmNode.OnHandleMessage(object msg)
		{
		}

		public IEnumerator Download()
		{
			// 请求游戏版本
			string webURL = _patcher.GetWebServerURL();
			string postContent = _patcher.GetWebPostContent();
			MotionLog.Log($"Beginning to request from web : {webURL}");
			MotionLog.Log($"Post content : {postContent}");
			WebPostRequest download = new WebPostRequest(webURL);
			int timeout = _patcher.GetGameVersionRequestTimeout();
			download.SendRequest(postContent, timeout);
			yield return download;

			// Check fatal
			if (download.HasError())
			{
				download.ReportError();
				download.Dispose();
				PatchEventDispatcher.SendGameVersionRequestFailedMsg();
				yield break;
			}

			string responseContent = download.GetResponse();
			MotionLog.Log($"Response content : {responseContent}");
			download.Dispose();

			// 如果解析成功
			if(_patcher.ParseResponseContent(responseContent))
				_patcher.SwitchNext();
			else
				PatchEventDispatcher.SendGameVersionParseFailedMsg();
		}
	}
}