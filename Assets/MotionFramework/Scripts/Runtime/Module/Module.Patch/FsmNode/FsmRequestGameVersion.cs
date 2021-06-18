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
			MotionLog.Log($"Web post content : {postContent}");
			WebPostRequest download = new WebPostRequest(webURL);
			download.SendRequest(postContent);
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
			download.Dispose();
			MotionLog.Log($"Succeed get response from web : {responseContent}");
			PatchEventDispatcher.SendGameVersionContentMsg(responseContent);
		}
	}
}