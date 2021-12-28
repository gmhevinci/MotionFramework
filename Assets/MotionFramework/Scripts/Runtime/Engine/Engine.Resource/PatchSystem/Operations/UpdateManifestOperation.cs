//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MotionFramework.Network;

namespace MotionFramework.Resource
{
	/// <summary>
	/// 更新清单操作
	/// </summary>
	public abstract class UpdateManifestOperation : AsyncOperationBase
	{
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal class EditorModeUpdateManifestOperation : UpdateManifestOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式的更新清单操作
	/// </summary>
	internal class OfflinePlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 网络模式的更新清单操作
	/// </summary>
	internal class HostPlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		private enum ESteps
		{
			Idle,
			LoadWebManifestHash,
			CheckWebManifestHash,
			LoadWebManifest,
			CheckWebManifest,
			InitPrepareCache,
			UpdatePrepareCache,
			Done,
		}

		private static int RequestCount = 0;

		private readonly HostPlayModeImpl _impl;
		private readonly int _updateResourceVersion;
		private readonly int _timeout;
		private ESteps _steps = ESteps.Idle;
		private WebGetRequest _downloaderHash;
		private WebGetRequest _downloaderManifest;


		public HostPlayModeUpdateManifestOperation(HostPlayModeImpl impl, int updateResourceVersion, int timeout)
		{
			_impl = impl;
			_updateResourceVersion = updateResourceVersion;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.LoadWebManifestHash;

			if (_impl.IgnoreResourceVersion && _updateResourceVersion > 0)
			{
				MotionLog.Warning($"Update resource version {_updateResourceVersion} is invalid when ignore resource version.");
			}
			else
			{
				MotionLog.Log($"Update patch manifest : update resource version is  {_updateResourceVersion}");
			}
		}
		internal override void Update()
		{
			if (_steps == ESteps.Idle)
				return;

			if (_steps == ESteps.LoadWebManifestHash)
			{
				string webURL = GetPatchManifestRequestURL(_updateResourceVersion, ResourceSettingData.Setting.PatchManifestHashFileName);
				MotionLog.Log($"Beginning to request patch manifest hash : {webURL}");
				_downloaderHash = new WebGetRequest(webURL);
				_downloaderHash.SendRequest(_timeout);
				_steps = ESteps.CheckWebManifestHash;
			}

			if (_steps == ESteps.CheckWebManifestHash)
			{
				if (_downloaderHash.IsDone() == false)
					return;

				// Check fatal
				if (_downloaderHash.HasError())
				{
					Error = _downloaderHash.GetError();
					Status = EOperationStatus.Failed;
					_downloaderHash.Dispose();
					_steps = ESteps.Done;
					return;
				}

				// 获取补丁清单文件的哈希值
				string webManifestHash = _downloaderHash.GetText();
				_downloaderHash.Dispose();

				// 如果补丁清单文件的哈希值相同
				string currentFileHash = PatchHelper.GetSandboxPatchManifestFileHash();
				if (currentFileHash == webManifestHash)
				{
					MotionLog.Log($"Patch manifest file hash is not change : {webManifestHash}");
					_steps = ESteps.InitPrepareCache;
				}
				else
				{
					MotionLog.Log($"Patch manifest hash is change : {webManifestHash} -> {currentFileHash}");
					_steps = ESteps.LoadWebManifest;
				}
			}

			if (_steps == ESteps.LoadWebManifest)
			{
				string webURL = GetPatchManifestRequestURL(_updateResourceVersion, ResourceSettingData.Setting.PatchManifestFileName);
				MotionLog.Log($"Beginning to request patch manifest : {webURL}");
				_downloaderManifest = new WebGetRequest(webURL);
				_downloaderManifest.SendRequest(_timeout);
				_steps = ESteps.CheckWebManifest;
			}

			if (_steps == ESteps.CheckWebManifest)
			{
				if (_downloaderManifest.IsDone() == false)
					return;

				// Check fatal
				if (_downloaderManifest.HasError())
				{
					Error = _downloaderManifest.GetError();
					Status = EOperationStatus.Failed;
					_downloaderManifest.Dispose();
					_steps = ESteps.Done;
					return;
				}

				// 解析补丁清单			
				ParseAndSaveRemotePatchManifest(_downloaderManifest.GetText());
				_downloaderManifest.Dispose();
				_steps = ESteps.InitPrepareCache;
			}

			if (_steps == ESteps.InitPrepareCache)
			{
				InitPrepareCache();
				_steps = ESteps.UpdatePrepareCache;
			}

			if (_steps == ESteps.UpdatePrepareCache)
			{
				if (UpdatePrepareCache())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
			}
		}

		private string GetPatchManifestRequestURL(int updateResourceVersion, string fileName)
		{
			string url;

			// 轮流返回请求地址
			if (RequestCount % 2 == 0)
				url = _impl.GetPatchDownloadFallbackURL(updateResourceVersion, fileName);
			else
				url = _impl.GetPatchDownloadMainURL(updateResourceVersion, fileName);

			// 注意：在URL末尾添加时间戳
			if (_impl.IgnoreResourceVersion)
				url = $"{url}?{System.DateTime.UtcNow.Ticks}";

			return url;
		}
		private void ParseAndSaveRemotePatchManifest(string content)
		{
			_impl.LocalPatchManifest = PatchManifest.Deserialize(content);

			// 注意：这里会覆盖掉沙盒内的补丁清单文件
			MotionLog.Log("Save remote patch manifest file.");
			string savePath = AssetPathHelper.MakePersistentLoadPath(ResourceSettingData.Setting.PatchManifestFileName);
			PatchManifest.Serialize(savePath, _impl.LocalPatchManifest);
		}

		#region 多线程相关
		private class ThreadInfo
		{
			public bool Result = false;
			public string FilePath { private set; get; }
			public PatchBundle Bundle { private set; get; }
			public ThreadInfo(string filePath, PatchBundle bundle)
			{
				FilePath = filePath;
				Bundle = bundle;
			}
		}

		private readonly List<PatchBundle> _cacheList = new List<PatchBundle>(1000);
		private readonly List<PatchBundle> _verifyList = new List<PatchBundle>(100);
		private readonly MainThreadSyncContext _syncContext = new MainThreadSyncContext();
		private const int VerifyMaxCount = 32;

		private void InitPrepareCache()
		{
			// 遍历所有文件然后验证并缓存合法文件
			foreach (var patchBundle in _impl.LocalPatchManifest.BundleList)
			{
				// 忽略缓存文件
				if (DownloadSystem.ContainsVerifyFile(patchBundle.Hash))
					continue;

				// 忽略APP资源
				// 注意：如果是APP资源并且哈希值相同，则不需要下载
				if (_impl.AppPatchManifest.Bundles.TryGetValue(patchBundle.BundleName, out PatchBundle appPatchBundle))
				{
					if (appPatchBundle.IsBuildin && appPatchBundle.Hash == patchBundle.Hash)
						continue;
				}

				_cacheList.Add(patchBundle);
			}
		}
		private bool UpdatePrepareCache()
		{
			_syncContext.Update();

			if (_cacheList.Count == 0 && _verifyList.Count == 0)
				return true;

			if (_verifyList.Count >= VerifyMaxCount)
				return false;

			for (int i = _cacheList.Count - 1; i >= 0; i--)
			{
				if (_verifyList.Count >= VerifyMaxCount)
					break;

				var patchBundle = _cacheList[i];
				if (RunThread(patchBundle))
				{
					_cacheList.RemoveAt(i);
					_verifyList.Add(patchBundle);
				}
				else
				{
					MotionLog.Log("Failed to run verify thread.");
					break;
				}
			}

			return false;
		}
		private bool RunThread(PatchBundle patchBundle)
		{
			string filePath = PatchHelper.MakeSandboxCacheFilePath(patchBundle.Hash);
			ThreadInfo info = new ThreadInfo(filePath, patchBundle);
			return ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyFile), info);
		}
		private void VerifyFile(object infoObj)
		{
			// 验证沙盒内的文件
			ThreadInfo info = (ThreadInfo)infoObj;
			info.Result = DownloadSystem.CheckContentIntegrity(info.FilePath, info.Bundle.SizeBytes, info.Bundle.CRC);
			_syncContext.Post(VerifyCallback, info);
		}
		private void VerifyCallback(object obj)
		{
			ThreadInfo info = (ThreadInfo)obj;
			if (info.Result)
				DownloadSystem.CacheVerifyFile(info.Bundle.Hash, info.Bundle.BundleName);
			_verifyList.Remove(info.Bundle);
		}
		#endregion
	}
}