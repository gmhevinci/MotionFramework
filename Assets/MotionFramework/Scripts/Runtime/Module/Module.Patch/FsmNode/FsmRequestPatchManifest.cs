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

			if (_patcher.IgnoreResourceVersion)
				MotionEngine.StartCoroutine(DownloadWithoutResourceVersion());
			else
				MotionEngine.StartCoroutine(DownloadByResourceVersion());
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

		private IEnumerator DownloadWithoutResourceVersion()
		{
			// 从远端请求补丁清单文件的哈希值，并比对沙盒内的补丁清单文件的哈希值
			{
				_requestCount++;
				string webURL = GetRequestURL(true, 0, PatchDefine.PatchManifestHashFileName);
				MotionLog.Log($"Beginning to request patch manifest hash : {webURL}");
				WebGetRequest download = new WebGetRequest(webURL);
				int timeout = _patcher.GetPatchManifestRequestTimeout();
				download.SendRequest(timeout);
				yield return download;

				// Check fatal
				if (download.HasError())
				{
					download.ReportError();
					download.Dispose();
					PatchEventDispatcher.SendPatchManifestRequestFailedMsg();
					yield break;
				}

				// 获取补丁清单文件的哈希值
				string patchManifestHash = download.GetText();
				download.Dispose();

				// 如果补丁清单文件的哈希值相同
				string currentFileHash = PatchHelper.GetSandboxPatchManifestFileHash();
				if (currentFileHash == patchManifestHash)
				{
					MotionLog.Log($"Patch manifest file hash is not change : {patchManifestHash}");
					_patcher.Switch(EPatchStates.PatchDone);
					yield break;
				}
				else
				{
					MotionLog.Log($"Patch manifest hash is change : {patchManifestHash} -> {currentFileHash}");
				}
			}

			// 从远端请求补丁清单
			{
				string webURL = GetRequestURL(true, 0, PatchDefine.PatchManifestFileName);
				MotionLog.Log($"Beginning to request patch manifest : {webURL}");
				WebGetRequest download = new WebGetRequest(webURL);
				int timeout = _patcher.GetPatchManifestRequestTimeout();
				download.SendRequest(timeout);
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
					_patcher.SwitchNext();
				}
			}
		}
		private IEnumerator DownloadByResourceVersion()
		{
			// 新安装的用户首次启动游戏（包括覆盖安装的用户）
			// 注意：请求的补丁清单会在下载流程结束的时候，自动保存在沙盒里。
			bool firstStartGame = PatchHelper.CheckSandboxPatchManifestFileExist() == false;
			if (firstStartGame)
				MotionLog.Log("First start game.");

			// 检测资源版本是否变化
			int newResourceVersion = _patcher.RequestedResourceVersion;
			int oldResourceVersion = _patcher.LocalResourceVersion;
			if (firstStartGame == false && newResourceVersion == oldResourceVersion)
			{
				MotionLog.Log($"Resource version is not change.");
				_patcher.Switch(EPatchStates.PatchDone);
			}
			else
			{
				// 从远端请求补丁清单
				_requestCount++;
				string webURL = GetRequestURL(false, newResourceVersion, PatchDefine.PatchManifestFileName);
				MotionLog.Log($"Beginning to request patch manifest : {webURL}");
				WebGetRequest download = new WebGetRequest(webURL);
				int timeout = _patcher.GetPatchManifestRequestTimeout();
				download.SendRequest(timeout);
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