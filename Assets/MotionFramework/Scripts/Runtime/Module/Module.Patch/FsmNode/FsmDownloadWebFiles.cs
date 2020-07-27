//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using MotionFramework.AI;

namespace MotionFramework.Patch
{
	internal class FsmDownloadWebFiles : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmDownloadWebFiles(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.DownloadWebFiles.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.DownloadWebFiles);
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
			// 注意：开发者需要在下载前检测磁盘空间不足

			// 注册下载回调
			var downloader = _patcher.Downloader;
			downloader.OnPatchFileCheckFailedCallback = PatchEventDispatcher.SendWebFileCheckFailedMsg;
			downloader.OnPatchFileDownloadFailedCallback = PatchEventDispatcher.SendWebFileDownloadFailedMsg;
			downloader.OnPatchFileDownloadSucceedCallback = PatchEventDispatcher.SendDownloadFilesProgressMsg;

			MotionLog.Log($"Begine download web files : {downloader.TotalDownloadCount} files and total {downloader.TotalDownloadBytes} bytes");
			yield return downloader.Download();

			// 检测下载结果
			if (downloader.DownloadStates != EDownloaderStates.DownloadSucceed)
				yield break;

			_patcher.OnDownloadWebPatchFile();
			_patcher.SwitchNext();
		}
	}
}