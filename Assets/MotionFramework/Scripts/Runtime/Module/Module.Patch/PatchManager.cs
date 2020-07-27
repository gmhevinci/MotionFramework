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
			/// 最近登录的服务器ID
			/// </summary>
			public int ServerID;

			/// <summary>
			/// 渠道ID
			/// </summary>
			public int ChannelID;

			/// <summary>
			/// 设备唯一ID
			/// </summary>
			public long DeviceID;

			/// <summary>
			/// 测试包标记
			/// </summary>
			public int TestFlag;

			/// <summary>
			/// 补丁文件校验等级
			/// </summary>
			public ECheckLevel CheckLevel = ECheckLevel.CheckSize;

			/// <summary>
			/// 远程服务器信息
			/// </summary>
			public RemoteServerInfo ServerInfo;

			/// <summary>
			/// 变体规则列表
			/// </summary>
			public List<VariantRule> VariantRules;
		}

		private PatchManagerImpl _patcher;
		private VariantCollector _variantCollector;
		private bool _isRun = false;


		void IModule.OnCreate(System.Object param)
		{
			CreateParameters createParam = param as CreateParameters;
			if (createParam == null)
				throw new Exception($"{nameof(PatchManager)} create param is invalid.");
			if (createParam.ServerInfo == null)
				throw new Exception("ServerInfo is null");

			// 注册变体规则
			if (createParam.VariantRules != null)
			{
				_variantCollector = new VariantCollector();
				foreach (var variantRule in createParam.VariantRules)
				{
					_variantCollector.RegisterVariantRule(variantRule.VariantGroup, variantRule.TargetVariant);
				}
			}

			// 创建补丁管理器实现类
			_patcher = new PatchManagerImpl();
			_patcher.Create(createParam);
		}
		void IModule.OnUpdate()
		{
			_patcher.Update();
		}
		void IModule.OnGUI()
		{
			ConsoleGUI.Lable($"[{nameof(PatchManager)}] States : {_patcher.CurrentStates}");
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAync()
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
		/// 获取DLC下载器
		/// </summary>
		public PatchDownloader CreateDLCDownloader(string dlcLabel)
		{
			return CreateDLCDownloader(new List<string>() { dlcLabel });
		}
		public PatchDownloader CreateDLCDownloader(List<string> dlcLabels)
		{
			var downloadList = _patcher.GetDLCDownloadList(dlcLabels);
			PatchDownloader downlader = new PatchDownloader(_patcher, downloadList);
			return downlader;
		}

		/// <summary>
		/// 接收事件
		/// </summary>
		public void HandleEventMessage(IEventMessage msg)
		{
			_patcher.HandleEventMessage(msg);
		}

		#region IBundleServices接口
		AssetBundleInfo IBundleServices.GetAssetBundleInfo(string manifestPath)
		{
			PatchManifest patchManifest = _patcher.GetPatchManifest();
			manifestPath = GetVariantManifestPath(patchManifest, manifestPath);
			return _patcher.GetAssetBundleInfo(manifestPath);
		}
		string[] IBundleServices.GetDirectDependencies(string assetBundleName)
		{
			PatchManifest patchManifest = _patcher.GetPatchManifest();
			return patchManifest.GetDirectDependencies(assetBundleName);
		}
		string[] IBundleServices.GetAllDependencies(string assetBundleName)
		{
			PatchManifest patchManifest = _patcher.GetPatchManifest();
			return patchManifest.GetAllDependencies(assetBundleName);
		}

		private string GetVariantManifestPath(PatchManifest patchManifest, string manifestPath)
		{
			if (_variantCollector == null)
				return manifestPath;

			if (patchManifest.HasVariant(manifestPath))
			{
				string variant = patchManifest.GetFirstVariant(manifestPath);
				return _variantCollector.TryGetVariantManifestPath(manifestPath, variant);
			}
			return manifestPath;
		}
		#endregion
	}
}