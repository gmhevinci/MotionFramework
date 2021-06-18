//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Resource;
using MotionFramework.Event;
using MotionFramework.Console;
using MotionFramework.Network;

namespace MotionFramework.Patch
{
	public sealed class PatchManager : ModuleSingleton<PatchManager>, IModule, IBundleServices
	{
		/// <summary>
		/// 游戏模块创建参数
		/// </summary>
		public class CreateParameters
		{
			/// <summary>
			/// 向WEB服务器投递的数据
			/// </summary>
			public string WebPoseContent;

			/// <summary>
			/// 补丁文件校验等级
			/// </summary>
			public EVerifyLevel VerifyLevel = EVerifyLevel.Size;

			/// <summary>
			/// 远程服务器信息
			/// </summary>
			public RemoteServerInfo ServerInfo;

			/// <summary>
			/// 首次启动游戏或更新游戏时自动下载的DLC列表
			/// </summary>
			public string[] AutoDownloadDLC;

			/// <summary>
			/// 首次启动游戏或更新游戏时自动下载内置DLC列表
			/// </summary>
			public bool AutoDownloadBuildinDLC;

			/// <summary>
			/// 同时下载的最大文件数（内置下载器参数）
			/// </summary>
			public int MaxNumberOnLoad = 1;

			/// <summary>
			/// 下载失败的重复次数（内置下载器参数）
			/// </summary>
			public int FailedTryAgain = 3;
		}

		private PatchManagerImpl _patcher;
		private bool _isRun = false;

		void IModule.OnCreate(System.Object param)
		{
			CreateParameters createParam = param as CreateParameters;
			if (createParam == null)
				throw new Exception($"{nameof(PatchManager)} create param is invalid.");
			if (createParam.ServerInfo == null)
				throw new Exception("ServerInfo is null");

			// 创建补丁管理器实现类
			_patcher = new PatchManagerImpl();
			_patcher.Create(createParam);
		}
		void IModule.OnUpdate()
		{
			_patcher.Update();		
			WebFileSystem.Update();
		}
		void IModule.OnGUI()
		{
			ConsoleGUI.Lable($"[{nameof(PatchManager)}] States : {_patcher.CurrentStates}");
			ConsoleGUI.Lable($"[{nameof(PatchManager)}] Reqeust : {WebFileSystem.GetRequestTotalCount()}");
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAsync()
		{
			yield return _patcher.InitializeAsync();
		}

		/// <summary>
		/// 开启补丁更新流程
		/// </summary>
		public void Download()
		{
			if (_isRun == false)
			{
				_isRun = true;
				_patcher.Download();
			}
		}

		/// <summary>
		/// 查询补丁系统是否更新结束
		/// </summary>
		public bool IsFinish()
		{
			return _patcher.CurrentStates == EPatchStates.PatchDone.ToString();
		}

		/// <summary>
		/// 清空缓存
		/// 注意：可以使用该方法修复我们本地的客户端
		/// </summary>
		public void ClearCache()
		{
			_patcher.ClearCache();
		}

		/// <summary>
		/// 获取请求的游戏版本号
		/// </summary>
		public Version GetRequestedGameVersion()
		{
			if (_patcher.RequestedGameVersion == null)
				return new Version(0, 0, 0, 0);
			return _patcher.RequestedGameVersion;
		}

		/// <summary>
		/// 获取请求的资源版本号
		/// </summary>
		public int GetRequestedResourceVersion()
		{
			return _patcher.RequestedResourceVersion;
		}

		/// <summary>
		/// 创建补丁下载器
		/// </summary>
		/// <param name="dlcTag">DLC标记</param>
		/// <param name="maxNumberOnLoad">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public PatchDownloader CreateDLCDownloader(string dlcTag, int maxNumberOnLoad, int failedTryAgain)
		{
			return CreateDLCDownloader(new string[] { dlcTag }, maxNumberOnLoad, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器
		/// </summary>
		/// <param name="dlcTags">DLC标记列表</param>
		/// <param name="maxNumberOnLoad">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public PatchDownloader CreateDLCDownloader(string[] dlcTags, int maxNumberOnLoad, int failedTryAgain)
		{
			if (dlcTags == null || dlcTags.Length == 0)
				throw new Exception("DLC tags is null or empty.");
			if (_isRun == false)
				throw new Exception($"The patch pipeline is not start. Call PatchManager.Instance.Download()");
			if (IsFinish() == false)
				throw new Exception($"The patch pipeline is not done.");

			var downloadList = _patcher.GetPatchDownloadList(dlcTags);
			PatchDownloader downlader = new PatchDownloader(_patcher, downloadList, maxNumberOnLoad, failedTryAgain);
			return downlader;
		}

		#region IBundleServices接口
		bool IBundleServices.CheckContentIntegrity(string bundleName)
		{
			bool result = _patcher.CheckContentIntegrity(bundleName);
			if (result)
			{
				_patcher.CacheDownloadPatchFile(bundleName);
			}		
			return result;
		}
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string bundleName)
		{
			return _patcher.GetAssetBundleInfo(bundleName);
		}
		string IBundleServices.GetAssetBundleName(string assetPath)
		{
			PatchManifest patchManifest = _patcher.GetPatchManifest();
			return patchManifest.GetAssetBundleName(assetPath);
		}
		string[] IBundleServices.GetDirectDependencies(string bundleName)
		{
			PatchManifest patchManifest = _patcher.GetPatchManifest();
			return patchManifest.GetDirectDependencies(bundleName);
		}
		string[] IBundleServices.GetAllDependencies(string bundleName)
		{
			PatchManifest patchManifest = _patcher.GetPatchManifest();
			return patchManifest.GetAllDependencies(bundleName);
		}
		#endregion
	}
}