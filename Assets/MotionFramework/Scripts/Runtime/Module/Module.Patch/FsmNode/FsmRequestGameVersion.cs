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
			string url = _patcher.GetWebServerURL();
			string post = _patcher.GetWebPostContent();
			MotionLog.Log($"Beginning to request from web : {url}");
			MotionLog.Log($"Web post content : {post}");
			WebPostRequest download = new WebPostRequest(url);
			download.SendRequest(post);
			yield return download;

			// Check fatal
			if (download.HasError())
			{
				download.ReportError();
				download.Dispose();
				PatchEventDispatcher.SendGameVersionRequestFailedMsg();
				yield break;
			}

			string response = download.GetResponse();
			MotionLog.Log($"Succeed get response from web : {url} {response}");
			bool result = _patcher.ParseWebResponse(response);
			download.Dispose();
			if (result)
				_patcher.SwitchNext();
			else
				PatchEventDispatcher.SendGameVersionParseFailedMsg();
		}
	}
}