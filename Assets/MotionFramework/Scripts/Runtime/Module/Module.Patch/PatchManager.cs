//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MotionFramework.Resource;
using MotionFramework.Event;
using MotionFramework.Console;
using MotionFramework.IO;

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

		private readonly PatchInitializer _initializer = new PatchInitializer();
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
			yield return _initializer.InitializeAync(_patcher);
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
		/// 修复客户端
		/// </summary>
		public void FixClient()
		{
			_patcher.FixClient();
		}

		/// <summary>
		/// 获取APP版本号
		/// </summary>
		public string GetAPPVersion()
		{
			if (_patcher.AppVersion == null)
				return "0.0.0.0";
			return _patcher.AppVersion.ToString();
		}

		/// <summary>
		/// 获取游戏版本号
		/// </summary>
		public string GetGameVersion()
		{
			if (_patcher.GameVersion == null)
				return "0.0.0.0";
			return _patcher.GameVersion.ToString();
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
			PatchManifest patchManifest = GetPatchManifest();
			manifestPath = GetVariantManifestPath(patchManifest, manifestPath);
			if (patchManifest.Elements.TryGetValue(manifestPath, out PatchElement element))
			{
				// 先查询APP内的资源
				if (_patcher.AppPatchManifest.Elements.TryGetValue(manifestPath, out PatchElement appElement))
				{
					if (appElement.MD5 == element.MD5)
					{
						string localPath = AssetPathHelper.MakeStreamingLoadPath(manifestPath);
						AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, localPath, string.Empty, element.MD5, element.SizeBytes, element.Version);
						return bundleInfo;
					}
				}

				// 如果APP里不存在或者MD5不匹配，则从沙盒里加载
				// 注意：如果沙盒内文件不存在，那么将会从服务器下载
				string sandboxLocalPath = AssetPathHelper.MakePersistentLoadPath(element.MD5);
				if (element.BackgroundDownload && File.Exists(sandboxLocalPath) == false)
				{
					string remoteURL = _patcher.GetWebDownloadURL(element.Version.ToString(), element.Name);
					AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, sandboxLocalPath, remoteURL, element.MD5, element.SizeBytes, element.Version);
					return bundleInfo;
				}
				else
				{
					AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, sandboxLocalPath, string.Empty, element.MD5, element.SizeBytes, element.Version);
					return bundleInfo;
				}
			}
			else
			{
				MotionLog.Warning($"Not found element in patch manifest : {manifestPath}");
				string loadPath = AssetPathHelper.MakeStreamingLoadPath(manifestPath);
				AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, loadPath);
				return bundleInfo;
			}
		}
		string[] IBundleServices.GetDirectDependencies(string assetBundleName)
		{
			PatchManifest patchManifest = GetPatchManifest();
			return patchManifest.GetDirectDependencies(assetBundleName);
		}
		string[] IBundleServices.GetAllDependencies(string assetBundleName)
		{
			PatchManifest patchManifest = GetPatchManifest();
			return patchManifest.GetAllDependencies(assetBundleName);
		}

		private PatchManifest GetPatchManifest()
		{
			if (_patcher.WebPatchManifest != null)
				return _patcher.WebPatchManifest;
			if (_patcher.SandboxPatchManifest != null)
				return _patcher.SandboxPatchManifest;
			return _patcher.AppPatchManifest;
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