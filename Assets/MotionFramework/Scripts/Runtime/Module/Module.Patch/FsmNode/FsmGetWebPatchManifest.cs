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
	internal class FsmGetWebPatchManifest : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmGetWebPatchManifest(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.GetWebPatchManifest.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.GetWebPatchManifest);
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
			// 从远端下载最新的补丁清单
			int newResourceVersion = _patcher.RequestedResourceVersion;
			string url = _patcher.GetWebDownloadURL(newResourceVersion.ToString(), PatchDefine.PatchManifestFileName);
			WebGetRequest download = new WebGetRequest(url);
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
			
			// 解析远端下载的补丁清单
			_patcher.ParseRemotePatchManifest(download.GetText());
			download.Dispose();
			_patcher.SwitchNext();
		}
	}
}