//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Network;
using MotionFramework.Utility;

namespace MotionFramework.Patch
{
	/// <summary>
	/// 补丁下载器
	/// </summary>
	public class PatchDownloader
	{
		public delegate void OnPatchFileDownloadSucceed(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes);
		public delegate void OnPatchFileDownloadFailed(string remoteURL, string fileName);
		public delegate void OnPatchFileCheckFailed(string fileName);
		
		private readonly PatchManagerImpl _patcher;
		private readonly List<PatchElement> _downloadList;

		public EDownloaderStates DownloadStates { private set; get; }
		public int TotalDownloadCount { private set; get; }
		public long TotalDownloadBytes { private set; get; }
		public int CurrentDownloadCount { private set; get; }
		public long CurrentDownloadBytes { private set; get; }

		public OnPatchFileDownloadSucceed OnPatchFileDownloadSucceedCallback { set; get; }
		public OnPatchFileDownloadFailed OnPatchFileDownloadFailedCallback { set; get; }
		public OnPatchFileCheckFailed OnPatchFileCheckFailedCallback { set; get; }


		private PatchDownloader()
		{
		}
		internal PatchDownloader(PatchManagerImpl patcher, List<PatchElement> downloadList)
		{
			_patcher = patcher;
			_downloadList = downloadList;

			DownloadStates = EDownloaderStates.Downloading;
			TotalDownloadCount = downloadList.Count;
			foreach (var element in downloadList)
			{
				TotalDownloadBytes += element.SizeBytes;
			}
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public IEnumerator Download()
		{
			if (DownloadStates != EDownloaderStates.Downloading)
				throw new System.Exception($"{nameof(PatchDownloader)} is download again.");

			// 如果下载列表为空
			if(_downloadList.Count == 0)
			{
				DownloadStates = EDownloaderStates.DownloadSucceed;
				yield break;
			}

			// 开始下载列表里的所有文件
			foreach (var element in _downloadList)
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
					DownloadStates = EDownloaderStates.DownloadFailed;
					OnPatchFileDownloadFailedCallback?.Invoke(url, element.Name);
					yield break;
				}

				// 立即释放加载器
				download.Dispose();
				CurrentDownloadCount++;
				CurrentDownloadBytes += element.SizeBytes;
				OnPatchFileDownloadSucceedCallback?.Invoke(TotalDownloadCount, CurrentDownloadCount, TotalDownloadBytes, CurrentDownloadBytes);
			}

			// 验证下载文件
			foreach (var element in _downloadList)
			{
				if (_patcher.CheckPatchFileValid(element) == false)
				{
					MotionLog.Error($"Download patch file is invalid : {element.Name}");
					DownloadStates = EDownloaderStates.DownloadFailed;
					OnPatchFileCheckFailedCallback?.Invoke(element.Name);
					yield break;
				}
			}

			// 更新缓存并保存
			_patcher.CacheDownloadPatchFiles(_downloadList);
			DownloadStates = EDownloaderStates.DownloadSucceed;
		}
	}
}