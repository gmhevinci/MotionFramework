//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.AI;
using MotionFramework.Resource;
using MotionFramework.Network;

namespace MotionFramework.Patch
{
	internal class FsmDownloadWebPatchManifest : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmDownloadWebPatchManifest(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.DownloadWebPatchManifest.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStatesChangeMsg(EPatchStates.DownloadWebPatchManifest);
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
			// 注意：等所有文件下载完毕后，下载并替换补丁清单
			int newResourceVersion = _patcher.RequestedResourceVersion;
			string url = _patcher.GetWebDownloadURL(newResourceVersion.ToString(), PatchDefine.PatchManifestBytesFileName);
			string savePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestBytesFileName);
			WebFileRequest download = new WebFileRequest(url, savePath);
			yield return download.DownLoad();

			if (download.States != EWebRequestStates.Success)
			{
				download.Dispose();
				PatchEventDispatcher.SendWebPatchManifestDownloadFailedMsg();
				yield break;
			}
			else
			{
				MotionLog.Log("Web patch manifest is download.");
				download.Dispose();
				_patcher.SwitchNext();
			}
		}
	}
}