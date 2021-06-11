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
	internal class FsmGetDownloadList : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmGetDownloadList(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.GetDownloadList.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.GetDownloadList);
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
			// 如果下载列表为空
			var downloadList = _patcher.GetAutoPatchDownloadList();
			if (downloadList.Count == 0)
			{
				MotionLog.Log("Not found update web files.");
				_patcher.Switch(EPatchStates.DownloadOver);
			}
			else
			{
				MotionLog.Log($"Found update web files : {downloadList.Count}");

				// 创建补丁下载器
				_patcher.CreateInternalDownloader(downloadList);

				// 发现新更新文件后，挂起流程系统
				// 注意：开发者需要在下载前检测磁盘空间不足
				int totalDownloadCount = _patcher.InternalDownloader.TotalDownloadCount;
				long totalDownloadBytes = _patcher.InternalDownloader.TotalDownloadBytes;
				PatchEventDispatcher.SendFoundUpdateFilesMsg(totalDownloadCount, totalDownloadBytes);
			}
		}
	}
}