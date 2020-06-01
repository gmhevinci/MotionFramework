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
			PatchEventDispatcher.SendPatchStatesChangeMsg(EPatchStates.RequestGameVersion);
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
			// 获取最新的游戏版本号
			{
				string url = _patcher.GetWebServerIP();
				string post = _patcher.GetWebPostData();
				PatchHelper.Log(ELogLevel.Log, $"Request game version : {url} : {post}");
				WebPostRequest download = new WebPostRequest(url, post);
				yield return download.DownLoad();

				//Check fatal
				if (download.States != EWebRequestStates.Success)
				{
					download.Dispose();
					PatchEventDispatcher.SendGameVersionRequestFailedMsg();
					yield break;
				}

				string response = download.GetResponse();
				_patcher.ParseResponseData(response);
				download.Dispose();
			}

			int newResourceVersion = _patcher.RequestedResourceVersion;
			int oldResourceVersion = _patcher.SandboxPatchManifest.Version;

			// 检测强更安装包
			if(_patcher.ForceInstall)
			{
				PatchHelper.Log(ELogLevel.Log, $"Found new APP can be install : {_patcher.GameVersion.ToString()}");
				PatchEventDispatcher.SendFoundForceInstallAPPMsg(_patcher.GameVersion.ToString(), _patcher.AppURL);
				yield break;
			}

			// 检测资源版本是否变化
			if (newResourceVersion == oldResourceVersion)
			{
				PatchHelper.Log(ELogLevel.Log, $"Resource version is not change.");
				_patcher.Switch(EPatchStates.DownloadOver.ToString());
			}
			else
			{
				PatchHelper.Log(ELogLevel.Log, $"Resource version is change : {oldResourceVersion} -> {newResourceVersion}");
				_patcher.SwitchNext();
			}
		}
	}
}