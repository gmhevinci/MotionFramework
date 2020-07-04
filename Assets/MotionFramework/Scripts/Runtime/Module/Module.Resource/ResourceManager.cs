//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using UnityEngine;
using MotionFramework.Console;
using MotionFramework.Utility;

namespace MotionFramework.Resource
{
	/// <summary>
	/// 资源管理器
	/// </summary>
	public sealed class ResourceManager : ModuleSingleton<ResourceManager>, IModule
	{
		/// <summary>
		/// 游戏模块创建参数
		/// </summary>
		public class CreateParameters
		{
			/// <summary>
			/// 资源定位的根路径
			/// 例如：Assets/MyResource
			/// </summary>
			public string LocationRoot;

			/// <summary>
			/// 在编辑器下模拟运行
			/// </summary>
			public bool SimulationOnEditor;

			/// <summary>
			/// AssetBundle服务接口
			/// </summary>
			public IBundleServices BundleServices;

			/// <summary>
			/// 文件解密服务器接口
			/// </summary>
			public IDecryptServices DecryptServices;

			/// <summary>
			/// 资源系统自动释放零引用资源的间隔秒数
			/// 注意：如果小于等于零代表不自动释放，可以使用ResourceManager.Release接口主动释放
			/// </summary>
			public float AutoReleaseInterval;
		}

		private Timer _releaseTimer;

		void IModule.OnCreate(System.Object param)
		{
			CreateParameters createParam = param as CreateParameters;
			if (createParam == null)
				throw new Exception($"{nameof(ResourceManager)} create param is invalid.");

			// 初始化资源系统
			AssetSystem.Initialize(createParam.LocationRoot, createParam.SimulationOnEditor, createParam.BundleServices, createParam.DecryptServices);

			// 创建间隔计时器
			if (createParam.AutoReleaseInterval > 0)
				_releaseTimer = Timer.CreatePepeatTimer(0, createParam.AutoReleaseInterval);
		}
		void IModule.OnUpdate()
		{
			// 轮询更新资源系统
			AssetSystem.UpdatePoll();

			// 自动释放零引用资源
			if (_releaseTimer != null && _releaseTimer.Update(Time.unscaledDeltaTime))
			{
				AssetSystem.Release();
			}
		}
		void IModule.OnGUI()
		{
			int totalCount = AssetSystem.GetLoaderCount();
			int failedCount = AssetSystem.GetLoaderFailedCount();
			ConsoleGUI.Lable($"[{nameof(ResourceManager)}] Virtual simulation : {AssetSystem.SimulationOnEditor}");
			ConsoleGUI.Lable($"[{nameof(ResourceManager)}] Loader total count : {totalCount}");
			ConsoleGUI.Lable($"[{nameof(ResourceManager)}] Loader failed count : {failedCount}");
		}

		/// <summary>
		/// 资源回收
		/// 卸载引用计数为零的资源
		/// </summary>
		public static void Release()
		{
			AssetSystem.Release();
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public void ForceReleaseAll()
		{
			AssetSystem.ForceReleaseAll();
		}

		/// <summary>
		/// 获取资源的最终加载路径
		/// </summary>
		public string GetLoadPath(string location, string variant = "unity3d")
		{
			if (AssetSystem.SimulationOnEditor)
			{
#if UNITY_EDITOR
				string loadPath = AssetPathHelper.FindDatabaseAssetPath(location);
				if (string.IsNullOrEmpty(loadPath))
					MotionLog.Warning($"Not found asset : {location}");
				return loadPath;
#else
				throw new Exception($"AssetSystem simulation only support unity editor.");
#endif
			}
			else
			{
				if (AssetSystem.BundleServices == null)
					throw new Exception($"{nameof(AssetSystem.BundleServices)} is null.");

				string manifestPath = AssetSystem.BundleServices.ConvertLocationToManifestPath(location, variant);
				string loadPath = AssetSystem.BundleServices.GetAssetBundleLoadPath(manifestPath);
				if (string.IsNullOrEmpty(loadPath))
					MotionLog.Warning($"Not found asset : {location}");
				return loadPath;
			}
		}
	}
}