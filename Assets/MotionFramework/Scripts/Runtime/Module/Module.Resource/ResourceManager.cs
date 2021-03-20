//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Console;
using MotionFramework.Utility;
using MotionFramework.Patch;

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
			/// 运行时的最大加载个数
			/// </summary>
			public int RuntimeMaxLoadingCount = int.MaxValue;

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
			AssetSystem.Initialize(createParam.LocationRoot, createParam.SimulationOnEditor, createParam.RuntimeMaxLoadingCount,
				createParam.BundleServices, createParam.DecryptServices);

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
				AssetSystem.UnloadUnusedAssets();
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
		public static void UnloadUnusedAssets()
		{
			AssetSystem.UnloadUnusedAssets();
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public void UnloadAllAssets()
		{
			AssetSystem.UnloadAllAssets();
		}

		/// <summary>
		/// 获取资源的信息
		/// </summary>
		public AssetBundleInfo GetAssetBundleInfo(string location)
		{
			return AssetSystem.GetAssetBundleInfo(location);
		}

		/// <summary>
		/// 释放资源对象
		/// </summary>
		public void Release(AssetOperationHandle handle)
		{
			handle.Release();
		}


		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : class
		{
			return LoadAssetInternal(location, typeof(TObject), true);
		}
		public AssetOperationHandle LoadAssetSync(System.Type type, string location)
		{
			return LoadAssetInternal(location, type, true);
		}

		/// <summary>
		/// 同步加载子资源对象集合
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public AssetOperationHandle LoadSubAssetsSync<TObject>(string location)
		{
			return LoadSubAssetsInternal(location, typeof(TObject), true);
		}
		public AssetOperationHandle LoadSubAssetsSync(System.Type type, string location)
		{
			return LoadSubAssetsInternal(location, type, true);
		}


		/// <summary>
		/// 异步加载场景
		/// </summary>
		public AssetOperationHandle LoadSceneAsync(string location, SceneInstanceParam instanceParam)
		{
			string sceneName = Path.GetFileName(location);
			AssetLoaderBase cacheLoader = AssetSystem.CreateLoader(location);
			var handle = cacheLoader.LoadSceneAsync(sceneName, instanceParam);
			return handle;
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string location)
		{
			return LoadAssetInternal(location, typeof(TObject), false);
		}
		public AssetOperationHandle LoadAssetAsync(System.Type type, string location)
		{
			return LoadAssetInternal(location, type, false);
		}

		/// <summary>
		/// 异步加载子资源对象集合
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public AssetOperationHandle LoadSubAssetsAsync<TObject>(string location)
		{
			return LoadSubAssetsInternal(location, typeof(TObject), false);
		}
		public AssetOperationHandle LoadSubAssetsAsync(System.Type type, string location)
		{
			return LoadSubAssetsInternal(location, type, false);
		}


		private AssetOperationHandle LoadAssetInternal(string location, System.Type assetType, bool forceSyncLoad)
		{
			string assetName = Path.GetFileName(location);
			AssetLoaderBase cacheLoader = AssetSystem.CreateLoader(location);
			var handle = cacheLoader.LoadAssetAsync(assetName, assetType);
			if (forceSyncLoad)
				cacheLoader.ForceSyncLoad();
			return handle;
		}
		private AssetOperationHandle LoadSubAssetsInternal(string location, System.Type assetType, bool forceSyncLoad)
		{
			string assetName = Path.GetFileName(location);
			AssetLoaderBase cacheLoader = AssetSystem.CreateLoader(location);
			var handle = cacheLoader.LoadSubAssetsAsync(assetName, assetType);
			if (forceSyncLoad)
				cacheLoader.ForceSyncLoad();
			return handle;
		}
	}
}