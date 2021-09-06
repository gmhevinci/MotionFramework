//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Patch;

namespace MotionFramework.Resource
{
	internal class BundleFileLoader
	{
		/// <summary>
		/// 资源文件信息
		/// </summary>
		public AssetBundleInfo BundleInfo { private set; get;  }

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { private set; get; }

		/// <summary>
		/// 加载状态
		/// </summary>
		public ELoaderStates States { private set; get; }

		/// <summary>
		/// 是否已经销毁
		/// </summary>
		public bool IsDestroyed { private set; get; } = false;


		private bool _isWaitForAsyncComplete = false;
		private FileDownloader _downloader;
		private AssetBundleCreateRequest _cacheRequest;
		internal AssetBundle CacheBundle { private set; get; }


		public BundleFileLoader(AssetBundleInfo bundleInfo)
		{
			BundleInfo = bundleInfo;
			RefCount = 0;
			States = ELoaderStates.None;
		}

		/// <summary>
		/// 引用（引用计数递加）
		/// </summary>
		public void Reference()
		{
			RefCount++;
		}

		/// <summary>
		/// 释放（引用计数递减）
		/// </summary>
		public void Release()
		{
			RefCount--;
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public void Update()
		{
			// 如果资源文件加载完毕
			if (IsDone())
				return;

			if (States == ELoaderStates.None)
			{
				// 检测加载地址是否为空
				if (string.IsNullOrEmpty(BundleInfo.LocalPath))
				{
					States = ELoaderStates.Fail;
					return;
				}

				if (string.IsNullOrEmpty(BundleInfo.RemoteURL))
					States = ELoaderStates.LoadFile;
				else
					States = ELoaderStates.Download;
			}

			// 1. 从服务器下载
			if (States == ELoaderStates.Download)
			{
				int failedTryAgain = 3;
				_downloader = DownloadSystem.GetFileDownloader(BundleInfo.RemoteURL, BundleInfo.RemoteFallbackURL, BundleInfo.LocalPath, failedTryAgain);
				States = ELoaderStates.CheckDownload;
			}

			// 2. 检测服务器下载结果
			if (States == ELoaderStates.CheckDownload)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_downloader.ReportError();
					States = ELoaderStates.Fail;
				}
				else
				{
					// 校验文件完整性
					if (AssetSystem.BundleServices.CheckContentIntegrity(BundleInfo.BundleName) == false)
					{
						MotionLog.Error($"Check download content integrity is failed : {BundleInfo.BundleName}");
						States = ELoaderStates.Fail;
					}
					else
					{
						States = ELoaderStates.LoadFile;
					}
				}

				// 释放网络资源下载器
				if (_downloader != null)
				{
					_downloader.Dispose();
					_downloader = null;
				}
			}

			// 3. 加载AssetBundle
			if (States == ELoaderStates.LoadFile)
			{
#if UNITY_EDITOR
				// 注意：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
				if (System.IO.File.Exists(BundleInfo.LocalPath) == false)
				{
					MotionLog.Warning($"Not found assetBundle file : {BundleInfo.LocalPath}");
					States = ELoaderStates.Fail;
					return;
				}
#endif

				// Load assetBundle file
				if (BundleInfo.IsEncrypted)
				{
					if (AssetSystem.DecryptServices == null)
						throw new Exception($"{nameof(BundleFileLoader)} need IDecryptServices : {BundleInfo.BundleName}");

					EDecryptMethod decryptType = AssetSystem.DecryptServices.DecryptType;
					if (decryptType == EDecryptMethod.GetDecryptOffset)
					{
						ulong offset = AssetSystem.DecryptServices.GetDecryptOffset(BundleInfo);
						if (_isWaitForAsyncComplete)
							CacheBundle = AssetBundle.LoadFromFile(BundleInfo.LocalPath, 0, offset);
						else
							_cacheRequest = AssetBundle.LoadFromFileAsync(BundleInfo.LocalPath, 0, offset);
					}
					else if (decryptType == EDecryptMethod.GetDecryptBinary)
					{
						byte[] binary = AssetSystem.DecryptServices.GetDecryptBinary(BundleInfo);
						if (_isWaitForAsyncComplete)
							CacheBundle = AssetBundle.LoadFromMemory(binary);
						else
							_cacheRequest = AssetBundle.LoadFromMemoryAsync(binary);
					}
					else
					{
						throw new NotImplementedException($"{decryptType}");
					}
				}
				else
				{
					if (_isWaitForAsyncComplete)
						CacheBundle = AssetBundle.LoadFromFile(BundleInfo.LocalPath);
					else
						_cacheRequest = AssetBundle.LoadFromFileAsync(BundleInfo.LocalPath);
				}
				States = ELoaderStates.CheckFile;
			}

			// 4. 检测AssetBundle加载结果
			if (States == ELoaderStates.CheckFile)
			{
				if (_cacheRequest != null)
				{
					if (_isWaitForAsyncComplete)
					{
						// 强制挂起主线程（注意：该操作会很耗时）
						MotionLog.Warning("Suspend the main thread to load unity bundle.");
						CacheBundle = _cacheRequest.assetBundle;
					}
					else
					{
						if (_cacheRequest.isDone == false)
							return;
						CacheBundle = _cacheRequest.assetBundle;
					}
				}

				// Check error
				if (CacheBundle == null)
				{
					MotionLog.Warning($"Failed to load assetBundle file : {BundleInfo.BundleName}");
					States = ELoaderStates.Fail;
				}
				else
				{
					States = ELoaderStates.Success;
				}
			}
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public void Destroy(bool checkFatal)
		{
			IsDestroyed = true;

			// Check fatal
			if (checkFatal)
			{
				if (RefCount > 0)
					throw new Exception($"Bundle file loader ref is not zero : {BundleInfo.BundleName}");
				if (IsDone() == false)
					throw new Exception($"Bundle file loader is not done : {BundleInfo.BundleName}");
			}

			if (_downloader != null)
			{
				_downloader.Dispose();
				_downloader = null;
			}

			if (CacheBundle != null)
			{
				CacheBundle.Unload(true);
				CacheBundle = null;
			}
		}

		/// <summary>
		/// 是否完毕（无论成功或失败）
		/// </summary>
		public bool IsDone()
		{
			return States == ELoaderStates.Success || States == ELoaderStates.Fail;
		}

		/// <summary>
		/// 是否可以销毁
		/// </summary>
		public bool CanDestroy()
		{
			if (IsDone() == false)
				return false;

			return RefCount <= 0;
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public void WaitForAsyncComplete()
		{
			_isWaitForAsyncComplete = true;

			int frame = 1000;
			while (true)
			{
				// 保险机制
				// 注意：如果需要从WEB端下载资源，可能会触发保险机制！
				frame--;
				if (frame == 0)
					throw new Exception($"WaitForAsyncComplete failed ! BundleName : {BundleInfo.BundleName} States : {States}");

				// 驱动流程
				Update();

				// 完成后退出
				if (IsDone())
					break;
			}
		}
	}
}