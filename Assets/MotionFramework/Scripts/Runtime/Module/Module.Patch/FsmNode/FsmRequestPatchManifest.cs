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
	internal class FsmRequestPatchManifest : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmRequestPatchManifest(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.RequestPatchManifest.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.RequestPatchManifest);
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
			// 检测资源版本是否变化
			int newResourceVersion = _patcher.RequestedResourceVersion;
			int oldResourceVersion = _patcher.LocalResourceVersion;
			if (newResourceVersion == oldResourceVersion)
			{
				MotionLog.Log($"Resource version is not change.");
				_patcher.Switch(EPatchStates.PatchDone);
			}
			else
			{
				// 从远端下载最新的补丁清单
				string url = _patcher.GetWebDownloadURL(newResourceVersion, PatchDefine.PatchManifestFileName);
				WebGetRequest download = new WebGetRequest(url);
				download.SendRequest();
				yield return download;

				// Check fatal
				if (download.HasError())
				{
					download.ReportError();
					download.Dispose();
					PatchEventDispatcher.SendPatchManifestRequestFailedMsg();
					yield break;
				}

				// 解析补丁清单
				_patcher.ParseRemotePatchManifest(download.GetText());
				download.Dispose();

				// 如果发现了新的安装包
				if (_patcher.FoundNewApp)
				{
					string requestedGameVersion = _patcher.RequestedGameVersion.ToString();
					MotionLog.Log($"Found new APP can be install : {requestedGameVersion}");
					PatchEventDispatcher.SendFoundNewAppMsg(_patcher.ForceInstall, _patcher.AppURL, requestedGameVersion);
				}
				else
				{
					MotionLog.Log($"Resource version is change : {oldResourceVersion} -> {newResourceVersion}");
					_patcher.SwitchNext();
				}
			}
		}
	}
}