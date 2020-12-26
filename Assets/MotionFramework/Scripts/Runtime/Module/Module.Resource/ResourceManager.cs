//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
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
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string location)
		{
			return LoadInternal(location, typeof(TObject), null);
		}
		public AssetOperationHandle LoadAssetAsync(System.Type type, string location)
		{
			return LoadInternal(location, type, null);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源对象相对路径</param>
		/// <param name="param">资源加载参数</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string location, IAssetParam param)
		{
			return LoadInternal(location, typeof(TObject), param);
		}
		public AssetOperationHandle LoadAssetAsync(System.Type type, string location, IAssetParam param)
		{
			return LoadInternal(location, type, param);
		}

		/// <summary>
		/// 释放资源对象
		/// </summary>
		public void Release(AssetOperationHandle handle)
		{
			handle.Release();
		}

		private AssetOperationHandle LoadInternal(string location, System.Type assetType, IAssetParam param)
		{
			string assetName = Path.GetFileName(location);
			AssetLoaderBase cacheLoader = AssetSystem.CreateLoader(location);
			return cacheLoader.LoadAssetAsync(assetName, assetType, param);
		}
	}
}