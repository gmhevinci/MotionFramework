//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MotionFramework.Network;
using MotionFramework.Utility;

namespace MotionFramework.Patch
{
	/// <summary>
	/// 补丁下载器
	/// </summary>
	public class PatchDownloader : IEnumerator
	{
		private const int MAX_LOADER_COUNT = 64;

		public delegate void OnDownloadOver(bool isSucceed);
		public delegate void OnDownloadProgress(int totalDownloadCount, int currentDownloadCoun, long totalDownloadBytes, long currentDownloadBytes);
		public delegate void OnPatchFileDownloadFailed(string fileName);
		public delegate void OnPatchFileCheckFailed(string fileName);

		private readonly PatchManagerImpl _patcherMgr;
		private readonly int _maxNumberOnLoad;
		private readonly int _failedTryAgain;
		private readonly List<PatchBundle> _downloadList;
		private readonly List<PatchBundle> _succeedList = new List<PatchBundle>();
		private readonly List<PatchBundle> _loadFailedList = new List<PatchBundle>();
		private readonly List<PatchBundle> _checkFailedList = new List<PatchBundle>();
		private readonly List<WebFileRequest> _downloaders = new List<WebFileRequest>();
		private readonly List<WebFileRequest> _removeList = new List<WebFileRequest>(MAX_LOADER_COUNT);

		// 数据相关
		public EDownloaderStates DownloadStates { private set; get; }
		public int TotalDownloadCount { private set; get; }
		public long TotalDownloadBytes { private set; get; }
		public int CurrentDownloadCount { private set; get; }
		public long CurrentDownloadBytes { private set; get; }
		private long _lastDownloadBytes = 0;
		private int _lastDownloadCount = 0;

		// 委托相关
		public OnDownloadOver OnDownloadOverCallback { set; get; }
		public OnDownloadProgress OnDownloadProgressCallback { set; get; }
		public OnPatchFileDownloadFailed OnPatchFileDownloadFailedCallback { set; get; }
		public OnPatchFileCheckFailed OnPatchFileCheckFailedCallback { set; get; }


		private PatchDownloader()
		{
		}
		internal PatchDownloader(PatchManagerImpl patcherMgr, List<PatchBundle> downloadList, int maxNumberOnLoad, int failedTryAgain)
		{
			_patcherMgr = patcherMgr;
			_downloadList = downloadList;
			_maxNumberOnLoad = UnityEngine.Mathf.Clamp(maxNumberOnLoad, 1, MAX_LOADER_COUNT); ;
			_failedTryAgain = failedTryAgain;

			DownloadStates = EDownloaderStates.None;
			TotalDownloadCount = downloadList.Count;
			foreach (var patchBundle in downloadList)
			{
				TotalDownloadBytes += patchBundle.SizeBytes;
			}
		}

		/// <summary>
		/// 是否完毕，无论成功或失败
		/// </summary>
		public bool IsDone()
		{
			return DownloadStates == EDownloaderStates.Failed || DownloadStates == EDownloaderStates.Succeed || DownloadStates == EDownloaderStates.Forbid;
		}

		/// <summary>
		/// 取消下载
		/// </summary>
		public void Forbid()
		{
			if (DownloadStates != EDownloaderStates.Forbid)
			{
				DownloadStates = EDownloaderStates.Forbid;
				foreach (var loader in _downloaders)
				{
					loader.Dispose();
				}
				_downloaders.Clear();
			}
		}

		/// <summary>
		/// 开始下载
		/// </summary>
		public void Download()
		{
			if (DownloadStates != EDownloaderStates.None)
				throw new System.Exception($"{nameof(PatchDownloader)} is already running.");

			MotionLog.Log($"Begine to download : {TotalDownloadCount} files and {TotalDownloadBytes} bytes");
			DownloadStates = EDownloaderStates.Loading;
		}

		/// <summary>
		/// 更新下载器
		/// </summary>
		public void Update()
		{
			if (DownloadStates != EDownloaderStates.Loading)
				return;

			// 检测下载器结果
			_removeList.Clear();
			long downloadBytes = CurrentDownloadBytes;
			foreach(var loader in _downloaders)
			{
				downloadBytes += (long)loader.DownloadedBytes;
				if (loader.IsDone() == false)
					continue;

				PatchBundle patchBundle = loader.UserData as PatchBundle;

				// 检测是否下载失败
				if (loader.HasError())
				{
					loader.ReportError();
					loader.Dispose();
					_removeList.Add(loader);
					_loadFailedList.Add(patchBundle);
					continue;
				}

				// 验证下载文件完整性
				if (_patcherMgr.CheckContentIntegrity(patchBundle) == false)
				{
					MotionLog.Error($"Check download content integrity is failed : {patchBundle.BundleName}");
					loader.Dispose();
					_removeList.Add(loader);
					_checkFailedList.Add(patchBundle);
					continue;
				}

				// 下载成功
				loader.Dispose();
				_removeList.Add(loader);
				_succeedList.Add(patchBundle);
				CurrentDownloadCount++;
				CurrentDownloadBytes += patchBundle.SizeBytes;
			}

			// 移除已经完成的下载器（无论成功或失败）
			foreach(var loader in _removeList)
			{
				_downloaders.Remove(loader);
			}

			// 如果下载进度发生变化
			if (_lastDownloadBytes != downloadBytes || _lastDownloadCount != CurrentDownloadCount)
			{
				_lastDownloadBytes = downloadBytes;
				_lastDownloadCount = CurrentDownloadCount;
				OnDownloadProgressCallback?.Invoke(TotalDownloadCount, _lastDownloadCount, TotalDownloadBytes, _lastDownloadBytes);
			}

			// 动态创建新的下载器到最大数量限制
			// 注意：如果期间有下载失败的文件，暂停动态创建下载器
			if (_downloadList.Count > 0 && _loadFailedList.Count == 0 && _checkFailedList.Count == 0)
			{
				if (_downloaders.Count < _maxNumberOnLoad)
				{
					int index = _downloadList.Count - 1;
					WebFileRequest downloader = CreateDownloader(_downloadList[index]);
					_downloaders.Add(downloader);
					_downloadList.RemoveAt(index);
				}
			}

			// 下载结算
			if (_downloaders.Count == 0)
			{
				// 更新缓存并保存
				if (_succeedList.Count > 0)
					_patcherMgr.CacheDownloadPatchFiles(_succeedList);

				if (_loadFailedList.Count > 0)
				{
					DownloadStates = EDownloaderStates.Failed;
					OnPatchFileDownloadFailedCallback?.Invoke(_loadFailedList[0].BundleName);
					OnDownloadOverCallback?.Invoke(false);
				}
				else if (_checkFailedList.Count > 0)
				{
					DownloadStates = EDownloaderStates.Failed;
					OnPatchFileCheckFailedCallback?.Invoke(_checkFailedList[0].BundleName);
					OnDownloadOverCallback?.Invoke(false);
				}
				else
				{
					// 结算成功
					DownloadStates = EDownloaderStates.Succeed;
					OnDownloadOverCallback?.Invoke(true);
				}
			}
		}

		private WebFileRequest CreateDownloader(PatchBundle patchBundle)
		{
			// 注意：资源版本号只用于确定下载路径
			string url = _patcherMgr.GetWebDownloadURL(patchBundle.Version, patchBundle.Hash);
			string savePath = PatchHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
			FileUtility.CreateFileDirectory(savePath);

			// 创建下载器
			MotionLog.Log($"Beginning to download web file : {patchBundle.BundleName} URL : {url}");
			WebFileRequest download = WebFileSystem.GetWebFileRequest(url, savePath, _failedTryAgain);
			download.UserData = patchBundle;
			return download;
		}

		#region 异步相关
		bool IEnumerator.MoveNext()
		{
			return !IsDone();
		}
		void IEnumerator.Reset()
		{
		}
		object IEnumerator.Current
		{
			get { return null; }
		}
		#endregion
	}
}