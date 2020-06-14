//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
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
			/// WEB服务器地址
			/// </summary>
			public Dictionary<RuntimePlatform, string> WebServers;

			/// <summary>
			/// CDN服务器地址
			/// </summary>
			public Dictionary<RuntimePlatform, string> CDNServers;

			/// <summary>
			/// 默认的Web服务器地址
			/// </summary>
			public string DefaultWebServerIP;

			/// <summary>
			/// 默认的CDN服务器地址
			/// </summary>
			public string DefaultCDNServerIP;

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

		/// <summary>
		/// 重新载入Unity清单
		/// 注意：在补丁更新结束之后，清单内容会发生变化。
		/// </summary>
		public void ReloadUnityManifest()
		{
			_unityManifest = LoadUnityManifest();
		}

		#region IBundleServices接口
		private string _cachedLocationRoot;
		private AssetBundleManifest _unityManifest;
		private AssetBundleManifest LoadUnityManifest()
		{
			IBundleServices bundleServices = this;
			string loadPath = bundleServices.GetAssetBundleLoadPath(PatchDefine.UnityManifestFileName);
			AssetBundle bundle = AssetBundle.LoadFromFile(loadPath);
			if (bundle == null)
				return null;

			AssetBundleManifest result = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
			bundle.Unload(false);
			return result;
		}

		string IBundleServices.ConvertLocationToManifestPath(string location, string variant)
		{
			if (_cachedLocationRoot == null)
			{
				if (string.IsNullOrEmpty(AssetSystem.LocationRoot))
					throw new System.Exception($"{nameof(AssetSystem.LocationRoot)} is null or empty.");
				_cachedLocationRoot = AssetSystem.LocationRoot.ToLower();
			}

			if (string.IsNullOrEmpty(variant))
				throw new System.Exception($"Variant is null or empty: {location}");

			return StringFormat.Format("{0}/{1}.{2}", _cachedLocationRoot, location.ToLower(), variant.ToLower());
		}
		string IBundleServices.GetAssetBundleLoadPath(string manifestPath)
		{
			PatchManifest patchManifest;
			if (_patcher.WebPatchManifest != null)
				patchManifest = _patcher.WebPatchManifest;
			else
				patchManifest = _patcher.SandboxPatchManifest;

			manifestPath = GetVariantManifestPath(patchManifest, manifestPath);
			if (patchManifest.Elements.TryGetValue(manifestPath, out PatchElement element))
			{
				// 先查询APP内的资源
				if (_patcher.AppPatchManifest.Elements.TryGetValue(manifestPath, out PatchElement appElement))
				{
					if (appElement.MD5 == element.MD5)
						return AssetPathHelper.MakeStreamingLoadPath(manifestPath);
				}

				// 如果APP里不存在或者MD5不匹配，则从沙盒里加载
				return AssetPathHelper.MakePersistentLoadPath(manifestPath);
			}
			else
			{
				PatchHelper.Log(ELogLevel.Warning, $"Not found element in patch manifest : {manifestPath}");
				return AssetPathHelper.MakeStreamingLoadPath(manifestPath);
			}
		}
		string[] IBundleServices.GetDirectDependencies(string assetBundleName)
		{
			if (_unityManifest == null)
				_unityManifest = LoadUnityManifest();
			return _unityManifest.GetDirectDependencies(assetBundleName);
		}
		string[] IBundleServices.GetAllDependencies(string assetBundleName)
		{
			if (_unityManifest == null)
				_unityManifest = LoadUnityManifest();
			return _unityManifest.GetAllDependencies(assetBundleName);
		}

		private string GetVariantManifestPath(PatchManifest patchManifest, string manifestPath)
		{
			if (_variantCollector == null)
				return manifestPath;

			if (patchManifest.Elements.TryGetValue(manifestPath, out PatchElement element))
			{
				// 如果是变体资源
				string variant = element.GetFirstVariant();
				if (string.IsNullOrEmpty(variant) == false)
					manifestPath = _variantCollector.TryGetVariantManifestPath(manifestPath, variant);
				return manifestPath;
			}
			else
			{
				PatchHelper.Log(ELogLevel.Warning, $"Not found element in patch manifest : {manifestPath}");
				return manifestPath;
			}
		}
		#endregion
	}
}