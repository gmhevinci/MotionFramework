//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.AI;
using MotionFramework.Network;

namespace MotionFramework.Patch
{
	internal class FsmParseWebPatchManifest : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmParseWebPatchManifest(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.ParseWebPatchManifest.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStatesChangeMsg(EPatchStates.ParseWebPatchManifest);
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

		private IEnumerator Download()
		{
			// 从网络上解析最新的补丁清单
			int newResourceVersion = _patcher.RequestedResourceVersion;
			string url = _patcher.GetWebDownloadURL(newResourceVersion.ToString(), PatchDefine.PatchManifestFileName);
			WebDataRequest download = new WebDataRequest(url);
			download.DownLoad();
			yield return download;

			// Check fatal
			if (download.HasError())
			{
				download.ReportError();
				download.Dispose();
				PatchEventDispatcher.SendWebPatchManifestDownloadFailedMsg();
				yield break;
			}

			MotionLog.Log($"Parse web patch manifest.");
			_patcher.ParseWebPatchManifest(download.GetText());
			download.Dispose();
			_patcher.SwitchNext();
		}
	}
}