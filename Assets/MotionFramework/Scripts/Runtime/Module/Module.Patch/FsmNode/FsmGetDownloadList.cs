//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.AI;
using MotionFramework.Resource;
using MotionFramework.Utility;

namespace MotionFramework.Patch
{
	internal class FsmGetDonwloadList : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmGetDonwloadList(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.GetDonwloadList.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStatesChangeMsg(EPatchStates.GetDonwloadList);
			GetDownloadList();
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

		private void GetDownloadList()
		{
			_patcher.DownloadList.Clear();

			// 临时下载列表
			List<PatchElement> downloadList = new List<PatchElement>(1000);

			// 准备下载列表
			foreach (var pair in _patcher.WebPatchManifest.Elements)
			{
				PatchElement remoteElement = pair.Value;

				// 忽略静默下载资源
				if (remoteElement.BackgroundDownload)
					continue;

				// 先检测APP里的清单
				if (_patcher.AppPatchManifest.Elements.TryGetValue(remoteElement.Name, out PatchElement appElement))
				{
					if (appElement.MD5 == remoteElement.MD5)
						continue;
				}

				// 再检测沙盒里的清单
				if (_patcher.SandboxPatchManifest.Elements.TryGetValue(remoteElement.Name, out PatchElement sandboxElement))
				{
					if (sandboxElement.MD5 != remoteElement.MD5)
						downloadList.Add(remoteElement);
				}
				else
				{
					downloadList.Add(remoteElement);
				}
			}

			// 检测已经存在的文件
			// 注意：如果玩家在加载过程中强制退出，下次再进入的时候跳过已经加载的文件
			List<string> removeList = new List<string>();
			foreach (var element in downloadList)
			{
				if (_patcher.CheckPatchFileValid(element))
					removeList.Add(element.Name);
			}
			foreach (var name in removeList)
			{
				for (int i = 0; i < downloadList.Count; i++)
				{
					if (downloadList[i].Name == name)
					{
						downloadList.RemoveAt(i);
						break;
					}
				}
			}

			// 如果下载列表为空
			if(downloadList.Count == 0)
			{
				_patcher.SwitchNext();
			}
			else
			{
				// 最后添加到正式下载列表里
				_patcher.DownloadList.AddRange(downloadList);
				downloadList.Clear();

				// 发现新更新文件后，挂起流程系统
				int totalDownloadCount = _patcher.DownloadList.Count;
				long totalDownloadSizeBytes = 0;
				foreach (var element in _patcher.DownloadList)
				{
					totalDownloadSizeBytes += element.SizeBytes;
				}
				PatchEventDispatcher.SendFoundUpdateFilesMsg(totalDownloadCount, totalDownloadSizeBytes);
			}
		}
	}
}