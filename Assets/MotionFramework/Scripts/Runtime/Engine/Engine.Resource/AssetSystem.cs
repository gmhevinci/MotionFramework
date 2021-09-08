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

namespace MotionFramework.Resource
{
	/// <summary>
	/// 基于定位的资源系统
	/// </summary>
	internal static class AssetSystem
	{
		private static readonly List<BundleFileLoader> _loaders = new List<BundleFileLoader>(1000);
		private static readonly List<AssetProviderBase> _providers = new List<AssetProviderBase>(1000);
		private static readonly List<string> _removeKeys = new List<string>(100);
		private static bool _isInitialize = false;


		/// <summary>
		/// 资源定位的根路径
		/// </summary>
		public static string LocationRoot { private set; get; }

		/// <summary>
		/// 在编辑器下模拟运行
		/// </summary>
		public static bool SimulationOnEditor { private set; get; }

		/// <summary>
		/// 运行时的最大加载个数
		/// </summary>
		public static int RuntimeMaxLoadingCount { private set; get; }

		/// <summary>
		/// AssetBundle服务接口
		/// </summary>
		public static IBundleServices BundleServices { private set; get; }

		/// <summary>
		/// 文件解密服务器接口
		/// </summary>
		public static IDecryptServices DecryptServices { private set; get; }


		/// <summary>
		/// 初始化资源系统
		/// 注意：在使用AssetSystem之前需要初始化
		/// </summary>
		public static void Initialize(string locationRoot, bool simulationOnEditor, int runtimeMaxLoadingCount, IBundleServices bundleServices, IDecryptServices decryptServices)
		{
			if (_isInitialize)
				throw new Exception($"{nameof(AssetSystem)} is already initialized");

			if (runtimeMaxLoadingCount < 3)
			{
				runtimeMaxLoadingCount = 3;
				MotionLog.Warning("AssetSystem RuntimeMaxLoadingCount minimum is 3");
			}

			_isInitialize = true;
			LocationRoot = AssetPathHelper.GetRegularPath(locationRoot);
			SimulationOnEditor = simulationOnEditor;
			RuntimeMaxLoadingCount = runtimeMaxLoadingCount;
			BundleServices = bundleServices;
			DecryptServices = decryptServices;
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public static void UpdatePoll()
		{
			// 更新加载器	
			foreach(var loader in _loaders)
			{
				loader.Update();
			}

			// 更新资源提供者
			// 注意：循环更新的时候，可能会扩展列表
			// 注意：不能限制场景对象的加载
			int loadingCount = 0;
			for(int i=0; i<_providers.Count; i++)
			{
				var provider = _providers[i];
				if (provider is SceneProvider || provider is EditorSceneProvider)
				{
					provider.Update();
				}
				else
				{
					if (loadingCount < RuntimeMaxLoadingCount)
						provider.Update();

					if (provider.IsDone == false)
						loadingCount++;
				}
			}

			// 销毁资源提供者
			for (int i = _providers.Count - 1; i >= 0; i--)
			{
				var provider = _providers[i];
				if (provider.CanDestroy())
				{
					provider.Destory();
					_providers.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public static void UnloadUnusedAssets()
		{
			for (int i = _loaders.Count - 1; i >= 0; i--)
			{
				BundleFileLoader loader = _loaders[i];
				if (loader.CanDestroy())
				{
					loader.Destroy(true);
					_loaders.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public static void ForceUnloadAllAssets()
		{
			foreach(var provider in _providers)
			{
				provider.Destory();
			}
			_providers.Clear();

			foreach (var loader in _loaders)
			{
				loader.Destroy(false);
			}
			_loaders.Clear();

			// 注意：调用底层接口释放所有资源
			Resources.UnloadUnusedAssets();
		}


		/// <summary>
		/// 定位地址转换为资源路径
		/// </summary>
		public static string ConvertLocationToAssetPath(string location)
		{
			if (SimulationOnEditor)
			{
#if UNITY_EDITOR
				return AssetPathHelper.FindDatabaseAssetPath(location);
#else
				throw new Exception($"AssetSystem simulation only support unity editor.");
#endif
			}
			else
			{
				return AssetPathHelper.CombineAssetPath(LocationRoot, location);
			}
		}

		/// <summary>
		/// 获取资源包信息
		/// </summary>
		public static AssetBundleInfo GetAssetBundleInfo(string assetPath)
		{
			if (SimulationOnEditor)
			{
				MotionLog.Warning($"{nameof(SimulationOnEditor)} mode can not get asset bundle info.");
				AssetBundleInfo bundleInfo = new AssetBundleInfo(assetPath, assetPath);
				return bundleInfo;
			}
			else
			{
				string bundleName = BundleServices.GetAssetBundleName(assetPath);
				return BundleServices.GetAssetBundleInfo(bundleName);
			}
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="scenePath">场景名称</param>
		public static AssetOperationHandle LoadSceneAsync(string scenePath, SceneInstanceParam instanceParam)
		{
			AssetProviderBase provider = TryGetProvider(scenePath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new EditorSceneProvider(scenePath, instanceParam);
				else
					provider = new SceneProvider(scenePath, instanceParam);
				_providers.Add(provider);
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetPath">资源路径</param>
		/// <param name="assetType">资源类型</param>
		public static AssetOperationHandle LoadAssetAsync(string assetPath, System.Type assetType)
		{
			AssetProviderBase provider = TryGetProvider(assetPath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new AssetDatabaseProvider(assetPath, assetType);
				else
					provider = new AssetBundleProvider(assetPath, assetType);
				_providers.Add(provider);
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}

		/// <summary>
		/// 异步加载所有子资源对象
		/// </summary>
		/// <param name="assetPath">资源路径</param>
		/// <param name="assetType">资源类型</param>、
		public static AssetOperationHandle LoadSubAssetsAsync(string assetPath, System.Type assetType)
		{
			AssetProviderBase provider = TryGetProvider(assetPath);
			if (provider == null)
			{
				if (SimulationOnEditor)
					provider = new AssetDatabaseSubProvider(assetPath, assetType);
				else
					provider = new AssetBundleSubProvider(assetPath, assetType);
				_providers.Add(provider);
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}


		/// <summary>
		/// 获取或创建一个资源包加载器
		/// </summary>
		internal static BundleFileLoader GetOrCreateBundleFileLoader(AssetBundleInfo bundleInfo)
		{
			// 如果加载器已经存在
			BundleFileLoader loader = TryGetLoader(bundleInfo.BundleName);
			if (loader != null)
				return loader;

			// 新增下载需求
			loader = new BundleFileLoader(bundleInfo);
			_loaders.Add(loader);
			return loader;
		}

		private static BundleFileLoader TryGetLoader(string bundleName)
		{
			BundleFileLoader loader = null;
			for (int i = 0; i < _loaders.Count; i++)
			{
				BundleFileLoader temp = _loaders[i];
				if (temp.BundleInfo.BundleName.Equals(bundleName))
				{
					loader = temp;
					break;
				}
			}
			return loader;
		}
		private static AssetProviderBase TryGetProvider(string assetPath)
		{
			AssetProviderBase provider = null;
			for (int i = 0; i < _providers.Count; i++)
			{
				AssetProviderBase temp = _providers[i];
				if (temp.AssetPath.Equals(assetPath))
				{
					provider = temp;
					break;
				}
			}
			return provider;
		}

		#region 调试专属方法
		internal static List<BundleFileLoader> GetAllLoaders()
		{
			return _loaders;
		}
		internal static int GetLoaderCount()
		{
			return _loaders.Count;
		}

		internal static List<AssetProviderBase> GetAllProviders()
		{
			return _providers;
		}
		internal static int GetProviderCount()
		{
			return _providers.Count;
		}
		#endregion
	}
}