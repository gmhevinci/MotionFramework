//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Console;

namespace MotionFramework.Resource
{
	public class PatchManager : ModuleSingleton<PatchManager>, IModule
	{
		public abstract class CreateParameters
		{
			/// <summary>
			/// 在编辑器下模拟运行
			/// </summary>
			public bool SimulationOnEditor;
		}

		/// <summary>
		/// 离线模式（本地打包运行模式）
		/// </summary>
		public class OfflinePlayModeParameters : CreateParameters
		{
		}

		/// <summary>
		/// 网络模式（网络打包运行模式）
		/// </summary>
		public class HostPlayModeParameters : CreateParameters
		{
			/// <summary>
			/// 当缓存池被污染的时候清理缓存池
			/// </summary>
			public bool ClearCacheWhenDirty;

			/// <summary>
			/// 忽略资源版本号
			/// </summary>
			public bool IgnoreResourceVersion;

			/// <summary>
			/// 下载文件校验等级
			/// </summary>
			public EVerifyLevel VerifyLevel;

			/// <summary>
			/// 默认的资源服务器下载地址
			/// </summary>
			public string DefaultHostServer;

			/// <summary>
			/// 备用的资源服务器下载地址
			/// </summary>
			public string FallbackHostServer;
		}

		/// <summary>
		/// 运行模式
		/// </summary>
		private enum ERunMode
		{
			/// <summary>
			/// 在编辑器下模拟
			/// </summary>
			SimulationOnEditor,

			/// <summary>
			/// 离线模式
			/// </summary>
			OfflinePlayMode,

			/// <summary>
			/// 网络模式
			/// </summary>
			HostPlayMode,
		}


		private ERunMode _runMode;
		private CreateParameters _createParameters;
		private OfflinePlayModeImpl _offlinePlayModeImpl;
		private HostPlayModeImpl _hostPlayModeImpl;

		/// <summary>
		/// 补丁包接口
		/// </summary>
		public IBundleServices BundleServices { private set; get; } = null;


		void IModule.OnCreate(System.Object param)
		{
			_createParameters = param as CreateParameters;
			if (_createParameters == null)
				throw new Exception($"{nameof(ResourceManager)} create param is invalid.");

			if (_createParameters.SimulationOnEditor)
			{
				_runMode = ERunMode.SimulationOnEditor;
			}
			else
			{
				if (_createParameters is OfflinePlayModeParameters)
					_runMode = ERunMode.OfflinePlayMode;
				else if (_createParameters is HostPlayModeParameters)
					_runMode = ERunMode.HostPlayMode;
				else
					throw new NotImplementedException();
			}
		}
		void IModule.OnUpdate()
		{
			// 更新下载管理系统
			DownloadSystem.Update();
		}
		void IModule.OnGUI()
		{
			ConsoleGUI.Lable($"[{nameof(PatchManager)}] Run Mode : {_runMode}");
			ConsoleGUI.Lable($"[{nameof(PatchManager)}] File Dwonloader : {DownloadSystem.GetFileDownloaderTotalCount()}");
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAsync()
		{
			if (_runMode == ERunMode.SimulationOnEditor)
			{
				yield break;
			}
			else if (_runMode == ERunMode.OfflinePlayMode)
			{
				var playModeImpl = new OfflinePlayModeImpl();
				yield return playModeImpl.InitializeAsync();
				_offlinePlayModeImpl = playModeImpl;
				BundleServices = playModeImpl;
			}
			else if (_runMode == ERunMode.HostPlayMode)
			{
				var playModeImpl = new HostPlayModeImpl();
				var hostPlayModeParameters = _createParameters as HostPlayModeParameters;
				yield return playModeImpl.InitializeAsync(
					hostPlayModeParameters.ClearCacheWhenDirty,
					hostPlayModeParameters.IgnoreResourceVersion,
					hostPlayModeParameters.VerifyLevel,
					hostPlayModeParameters.DefaultHostServer,
					hostPlayModeParameters.FallbackHostServer);
				_hostPlayModeImpl = playModeImpl;
				BundleServices = playModeImpl;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 更新补丁清单
		/// </summary>
		/// <param name="updateResourceVersion">更新的资源版本号</param>
		/// <param name="timeout">超时时间</param>
		public IEnumerator UpdateManifestAsync(int updateResourceVersion, int timeout)
		{
			if (_runMode == ERunMode.SimulationOnEditor)
			{
				yield break;
			}
			else if (_runMode == ERunMode.OfflinePlayMode)
			{
				yield break;
			}
			else if (_runMode == ERunMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("PatchManager is not initialized.");
				yield return _hostPlayModeImpl.UpdatePatchManifestAsync(updateResourceVersion, timeout);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 补丁清单更新结果
		/// </summary>
		public UpdateManifestResult GetUpdateManifestResult()
		{
			if (_runMode == ERunMode.SimulationOnEditor)
			{
				return UpdateManifestResult.CreateSucceedResult();
			}
			else if (_runMode == ERunMode.OfflinePlayMode)
			{
				return UpdateManifestResult.CreateSucceedResult();
			}
			else if (_runMode == ERunMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("PatchManager is not initialized.");
				return _hostPlayModeImpl.ManifestResult;
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 获取资源版本号
		/// </summary>
		public int GetResourceVersion()
		{
			if (_runMode == ERunMode.SimulationOnEditor)
			{
				return 0;
			}
			else if (_runMode == ERunMode.OfflinePlayMode)
			{
				if (_offlinePlayModeImpl == null)
					throw new Exception("PatchManager is not initialized.");
				return _offlinePlayModeImpl.GetResourceVersion();
			}
			else if (_runMode == ERunMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("PatchManager is not initialized.");
				return _hostPlayModeImpl.GetResourceVersion();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 获取内置资源标记列表
		/// </summary>
		public string[] GetManifestBuildinTags()
		{
			if (_runMode == ERunMode.SimulationOnEditor)
			{
				return null;
			}
			else if (_runMode == ERunMode.OfflinePlayMode)
			{
				if (_offlinePlayModeImpl == null)
					throw new Exception("PatchManager is not initialized.");
				return _offlinePlayModeImpl.GetManifestBuildinTags();
			}
			else if (_runMode == ERunMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("PatchManager is not initialized.");
				return _hostPlayModeImpl.GetManifestBuildinTags();
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 创建补丁下载器
		/// </summary>
		/// <param name="dlcTag">DLC标记</param>
		/// <param name="fileLoadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public PatchDownloader CreateDLCDownloader(string dlcTag, int fileLoadingMaxNumber, int failedTryAgain)
		{
			return CreateDLCDownloader(new string[] { dlcTag }, fileLoadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器
		/// </summary>
		/// <param name="dlcTags">DLC标记列表</param>
		/// <param name="fileLoadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public PatchDownloader CreateDLCDownloader(string[] dlcTags, int fileLoadingMaxNumber, int failedTryAgain)
		{
			if (_runMode == ERunMode.SimulationOnEditor)
			{
				List<PatchBundle> downloadList = new List<PatchBundle>();
				PatchDownloader downlader = new PatchDownloader(null, downloadList, fileLoadingMaxNumber, failedTryAgain);
				return downlader;
			}
			else if (_runMode == ERunMode.OfflinePlayMode)
			{
				List<PatchBundle> downloadList = new List<PatchBundle>();
				PatchDownloader downlader = new PatchDownloader(null, downloadList, fileLoadingMaxNumber, failedTryAgain);
				return downlader;
			}
			else if (_runMode == ERunMode.HostPlayMode)
			{
				if (_hostPlayModeImpl == null)
					throw new Exception("PatchManager is not initialized.");
				return _hostPlayModeImpl.CreateDLCDownloader(dlcTags, fileLoadingMaxNumber, failedTryAgain);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// 清空沙盒目录
		/// 注意：可以使用该方法修复我们本地的客户端
		/// </summary>
		public void ClearSandbox()
		{
			MotionLog.Warning("Clear sandbox.");
			PatchHelper.ClearSandbox();
		}
	}
}