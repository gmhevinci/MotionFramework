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
			if (_patcher.InternalDownloader != null)
				_patcher.InternalDownloader.Update();
		}
		void IFsmNode.OnExit()
		{
		}
		void IFsmNode.OnHandleMessage(object msg)
		{
		}

		private IEnumerator Download()
		{
			var downloader = _patcher.InternalDownloader;

			// 注册下载回调
			downloader.OnPatchFileCheckFailedCallback = PatchEventDispatcher.SendWebFileCheckFailedMsg;
			downloader.OnPatchFileDownloadFailedCallback = PatchEventDispatcher.SendWebFileDownloadFailedMsg;
			downloader.OnPatchFileDownloadSucceedCallback = PatchEventDispatcher.SendDownloadFilesProgressMsg;
			downloader.Download();
			yield return downloader;

			// 检测下载结果
			if (downloader.DownloadStates != EDownloaderStates.Succeed)
				yield break;

			_patcher.SwitchNext();
		}
	}
}