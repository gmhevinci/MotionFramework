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

		public delegate void OnPatchFileDownloadSucceed(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes);
		public delegate void OnPatchFileDownloadFailed(string fileName);
		public delegate void OnPatchFileCheckFailed(string fileName);

		private readonly PatchManagerImpl _patcher;
		private readonly int _maxNumberOnLoad;
		private readonly List<PatchElement> _downloadList;
		private readonly List<PatchElement> _succeedList = new List<PatchElement>();
		private readonly List<PatchElement> _loadFailedList = new List<PatchElement>();
		private readonly List<PatchElement> _checkFailedList = new List<PatchElement>();
		private readonly List<WebFileRequest> _loaders = new List<WebFileRequest>();

		// 数据相关
		public EDownloaderStates DownloadStates { private set; get; }
		public int TotalDownloadCount { private set; get; }
		public long TotalDownloadBytes { private set; get; }
		public int CurrentDownloadCount { private set; get; }
		public long CurrentDownloadBytes { private set; get; }

		// 委托相关
		public OnPatchFileDownloadSucceed OnPatchFileDownloadSucceedCallback { set; get; }
		public OnPatchFileDownloadFailed OnPatchFileDownloadFailedCallback { set; get; }
		public OnPatchFileCheckFailed OnPatchFileCheckFailedCallback { set; get; }


		private PatchDownloader()
		{
		}
		internal PatchDownloader(PatchManagerImpl patcher, List<PatchElement> downloadList, int maxNumberOnLoad)
		{
			_patcher = patcher;
			_downloadList = downloadList;
			_maxNumberOnLoad = UnityEngine.Mathf.Clamp(maxNumberOnLoad, 1, MAX_LOADER_COUNT); ;

			DownloadStates = EDownloaderStates.None;
			TotalDownloadCount = downloadList.Count;
			foreach (var element in downloadList)
			{
				TotalDownloadBytes += element.SizeBytes;
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
		/// 释放下载器
		/// </summary>
		public void Dispose()
		{
			if(DownloadStates != EDownloaderStates.Forbid)
			{
				DownloadStates = EDownloaderStates.Forbid;
				foreach(var loader in _loaders)
				{
					loader.Dispose();
				}
				_loaders.Clear();
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
			for (int i = _loaders.Count - 1; i >= 0; i--)
			{
				var loader = _loaders[i];
				if (loader.IsDone() == false)
					continue;

				PatchElement element = loader.UserData as PatchElement;

				// 检测是否下载失败
				if (loader.HasError())
				{
					loader.ReportError();
					loader.Dispose();
					_loaders.RemoveAt(i);
					_loadFailedList.Add(element);
					continue;
				}

				// 验证下载文件完整性
				if (_patcher.CheckContentIntegrity(element) == false)
				{
					MotionLog.Error($"Check download content integrity is failed : {element.BundleName}");
					loader.Dispose();
					_loaders.RemoveAt(i);
					_checkFailedList.Add(element);			
					continue;
				}

				// 下载成功
				loader.Dispose();
				_loaders.RemoveAt(i);
				_succeedList.Add(element);
				CurrentDownloadCount++;
				CurrentDownloadBytes += element.SizeBytes;
				OnPatchFileDownloadSucceedCallback?.Invoke(TotalDownloadCount, CurrentDownloadCount, TotalDownloadBytes, CurrentDownloadBytes);
			}

			// 动态创建新的下载器到最大数量限制
			// 注意：如果期间有下载失败的文件，暂停动态创建下载器
			if (_downloadList.Count > 0 && _loadFailedList.Count == 0 && _checkFailedList.Count == 0)
			{
				if (_loaders.Count < _maxNumberOnLoad)
				{
					int index = _downloadList.Count - 1;
					WebFileRequest downloader = CreateDownloader(_downloadList[index]);
					_loaders.Add(downloader);
					_downloadList.RemoveAt(index);
				}
			}

			// 下载结算
			if (_loaders.Count == 0)
			{
				// 更新缓存并保存
				if (_succeedList.Count > 0)
					_patcher.CacheDownloadPatchFiles(_succeedList);

				if (_loadFailedList.Count > 0)
				{
					DownloadStates = EDownloaderStates.Failed;
					OnPatchFileDownloadFailedCallback?.Invoke(_loadFailedList[0].BundleName);
				}
				else if(_checkFailedList.Count > 0)
				{
					DownloadStates = EDownloaderStates.Failed;
					OnPatchFileCheckFailedCallback?.Invoke(_checkFailedList[0].BundleName);			
				}
				else
				{
					// 结算成功
					DownloadStates = EDownloaderStates.Succeed;
				}
			}
		}

		private WebFileRequest CreateDownloader(PatchElement element)
		{
			// 注意：资源版本号只用于确定下载路径
			string url = _patcher.GetWebDownloadURL(element.Version.ToString(), element.MD5);
			string savePath = PatchHelper.MakeSandboxCacheFilePath(element.MD5);
			FileUtility.CreateFileDirectory(savePath);

			// 创建下载器
			MotionLog.Log($"Beginning to download web file : {url}");
			WebFileRequest download = new WebFileRequest(url, savePath);
			download.UserData = element;
			download.DownLoad();
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