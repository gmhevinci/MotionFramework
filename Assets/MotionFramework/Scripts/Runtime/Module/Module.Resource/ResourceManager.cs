//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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
			/// 在编辑器下模拟运行
			/// </summary>
			public bool SimulationOnEditor;

			/// <summary>
			/// 资源定位的根路径
			/// 例如：Assets/MyResource
			/// </summary>
			public string LocationRoot;

			/// <summary>
			/// 资源加载的最大数量
			/// </summary>
			public int AssetLoadingMaxNumber;

			/// <summary>
			/// 文件解密接口
			/// </summary>
			public IDecryptServices DecryptServices;

			/// <summary>
			/// 资源包接口
			/// </summary>
			public IBundleServices BundleServices;

			/// <summary>
			/// 资源系统自动释放零引用资源的间隔秒数
			/// 注意：如果小于等于零代表不自动释放，可以使用ResourceManager.UnloadUnusedAssets接口主动释放
			/// </summary>
			public float AutoReleaseInterval;
		}

		private Timer _releaseTimer;


		void IModule.OnCreate(System.Object param)
		{
			CreateParameters createParam = param as CreateParameters;
			if (createParam == null)
				throw new Exception($"{nameof(ResourceManager)} create param is invalid.");

			if (createParam.AssetLoadingMaxNumber < 3)
			{
				createParam.AssetLoadingMaxNumber = 3;
				MotionLog.Warning($"{nameof(createParam.AssetLoadingMaxNumber)} minimum is 3");
			}

			// 创建间隔计时器
			if (createParam.AutoReleaseInterval > 0)
				_releaseTimer = Timer.CreatePepeatTimer(0, createParam.AutoReleaseInterval);

			// 初始化资源系统
			AssetSystem.Initialize(createParam.SimulationOnEditor, createParam.LocationRoot,
				createParam.AssetLoadingMaxNumber, createParam.DecryptServices, createParam.BundleServices);
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
			ConsoleGUI.Lable($"[{nameof(ResourceManager)}] Bundle count : {AssetSystem.GetLoaderCount()}");
			ConsoleGUI.Lable($"[{nameof(ResourceManager)}] Asset loader count : {AssetSystem.GetProviderCount()}");
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public void UnloadUnusedAssets()
		{
			AssetSystem.UpdatePoll();
			AssetSystem.UnloadUnusedAssets();
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public void ForceUnloadAllAssets()
		{
			AssetSystem.ForceUnloadAllAssets();
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		public AssetBundleInfo GetAssetBundleInfo(string location)
		{
			string assetPath = AssetSystem.ConvertLocationToAssetPath(location);
			return AssetSystem.GetAssetBundleInfo(assetPath);
		}

		/// <summary>
		/// 释放资源对象
		/// </summary>
		public void Release(AssetOperationHandle handle)
		{
			handle.Release();
		}

		#region 资源加载接口
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
			string scenePath = AssetSystem.ConvertLocationToAssetPath(location);
			var handle = AssetSystem.LoadSceneAsync(scenePath, instanceParam);
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

		private AssetOperationHandle LoadAssetInternal(string location, System.Type assetType, bool waitForAsyncComplete)
		{
			string assetPath = AssetSystem.ConvertLocationToAssetPath(location);
			var handle = AssetSystem.LoadAssetAsync(assetPath, assetType);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		private AssetOperationHandle LoadSubAssetsInternal(string location, System.Type assetType, bool waitForAsyncComplete)
		{
			string assetPath = AssetSystem.ConvertLocationToAssetPath(location);
			var handle = AssetSystem.LoadSubAssetsAsync(assetPath, assetType);
			if (waitForAsyncComplete)
				handle.WaitForAsyncComplete();
			return handle;
		}
		#endregion
	}
}