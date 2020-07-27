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
using MotionFramework.Utility;

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

			int totalDownloadCount = _patcher.GetDownloadTotalCount();
			long totalDownloadSizeBytes = _patcher.GetDownloadTotalSize();
			int currentDownloadCount = 0;
			long currentDownloadSizeBytes = 0;

			// 开始下载列表里的所有资源
			MotionLog.Log($"Begine download web files : {totalDownloadCount} count and total {totalDownloadSizeBytes} bytes");		
			foreach (var element in _patcher.DownloadList)
			{
				// 注意：资源版本号只用于确定下载路径
				string url = _patcher.GetWebDownloadURL(element.Version.ToString(), element.Name);
				string savePath = PatchHelper.MakeSandboxCacheFilePath(element.MD5);
				FileUtility.CreateFileDirectory(savePath);

				// 创建下载器
				MotionLog.Log($"Beginning to download web file : {url}");
				WebFileRequest download = new WebFileRequest(url, savePath);
				download.DownLoad();
				yield return download; //文件依次加载（在一个文件加载完毕后加载下一个）				
				
				// 检测是否下载失败
				if (download.HasError())
				{
					download.ReportError();
					download.Dispose();
					PatchEventDispatcher.SendWebFileDownloadFailedMsg(url, element.Name);
					yield break;
				}

				// 立即释放加载器
				download.Dispose();
				currentDownloadCount++;
				currentDownloadSizeBytes += element.SizeBytes;
				PatchEventDispatcher.SendDownloadFilesProgressMsg(totalDownloadCount, currentDownloadCount, totalDownloadSizeBytes, currentDownloadSizeBytes);
			}

			// 验证下载文件
			foreach (var element in _patcher.DownloadList)
			{
				if (_patcher.Cache.CheckPatchFileValid(element) == false)
				{
					MotionLog.Error($"Patch file is invalid : {element.Name}");
					PatchEventDispatcher.SendWebFileCheckFailedMsg(element.Name);
					yield break;
				}
			}

			// 更新缓存并保存
			_patcher.Cache.OnDownloadRemotePatchFile(_patcher.DownloadList);

			// 最后清空下载列表
			_patcher.ClearDownloadList();
			_patcher.SwitchNext();
		}
	}
}