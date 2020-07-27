//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.AI;

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
			PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.GetDonwloadList);
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
			_patcher.DownloadList = _patcher.Cache.GetDownloadList();

			// 如果下载列表为空
			if (_patcher.DownloadList.Count == 0)
			{
				_patcher.SwitchNext();
			}
			else
			{
				// 发现新更新文件后，挂起流程系统
				int totalDownloadCount = _patcher.GetDownloadTotalCount();
				long totalDownloadSizeBytes = _patcher.GetDownloadTotalSize();
				PatchEventDispatcher.SendFoundUpdateFilesMsg(totalDownloadCount, totalDownloadSizeBytes);
			}
		}
	}
}