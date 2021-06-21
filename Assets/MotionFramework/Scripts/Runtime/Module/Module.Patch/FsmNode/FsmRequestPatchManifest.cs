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
		private int _requestCount = 0;

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
			// 如果忽略资源版本，那么每次启动都会下载补丁清单
			bool ignoreResourceVersion = _patcher.IgnoreResourceVersion;

			// 新安装的用户首次启动游戏（包括覆盖安装的用户）
			// 注意：请求的补丁清单会在下载流程结束的时候，自动保存在沙盒里。
			bool firstStartGame = PatchHelper.CheckSandboxPatchManifestFileExist() == false;

			// 检测资源版本是否变化
			int newResourceVersion = _patcher.RequestedResourceVersion;
			int oldResourceVersion = _patcher.LocalResourceVersion;
			if (ignoreResourceVersion == false && firstStartGame == false && newResourceVersion == oldResourceVersion)
			{
				MotionLog.Log($"Resource version is not change.");
				_patcher.Switch(EPatchStates.PatchDone);
			}
			else
			{
				// 从远端请求补丁清单
				_requestCount++;
				string url = GetRequestURL(ignoreResourceVersion, newResourceVersion, PatchDefine.PatchManifestFileName);
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
					if (firstStartGame)
						MotionLog.Log("First start game.");
					if (newResourceVersion != oldResourceVersion)
						MotionLog.Log($"Resource version is change : {oldResourceVersion} -> {newResourceVersion}");
					_patcher.SwitchNext();
				}
			}
		}
		private string GetRequestURL(bool ignoreResrouceVersion, int resourceVersion, string fileName)
		{
			string url;

			// 轮流返回请求地址
			if (_requestCount % 2 == 0)
				url = _patcher.GetPatchDownloadFallbackURL(resourceVersion, fileName);
			else
				url = _patcher.GetPatchDownloadURL(resourceVersion, fileName);

			// 注意：在URL末尾添加时间戳
			if (ignoreResrouceVersion)
				url = $"{url}?{System.DateTime.UtcNow.Ticks}";

			return url;
		}
	}
}