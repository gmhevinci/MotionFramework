﻿//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;
using System;

namespace MotionFramework.Resource
{
	internal abstract class FileLoaderBase
	{
		/// <summary>
		/// 资源文件信息
		/// </summary>
		public AssetBundleInfo BundleInfo { get; private set; }

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { get; private set; }

		/// <summary>
		/// 加载状态
		/// </summary>
		public ELoaderStates States { get; protected set; }

		/// <summary>
		/// 是否为场景加载器
		/// </summary>
		public bool IsSceneLoader { private set; get; } = false;

		/// <summary>
		/// 是否已经销毁
		/// </summary>
		public bool IsDestroyed { private set; get; } = false;


		public FileLoaderBase(AssetBundleInfo bundleInfo)
		{
			BundleInfo = bundleInfo;
			RefCount = 0;
			States = ELoaderStates.None;
		}

		/// <summary>
		/// 引用（引用计数递加）
		/// </summary>
		protected virtual void Reference()
		{
			RefCount++;
		}

		/// <summary>
		/// 释放（引用计数递减）
		/// </summary>
		protected virtual void Release()
		{
			RefCount--;
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public virtual void Update()
		{
			if(CheckFileLoadDone())
			{
				UpdateProviders();
			}
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public virtual void Destroy(bool checkFatal)
		{
			IsDestroyed = true;
		}

		/// <summary>
		/// 是否可以销毁
		/// </summary>
		public bool CanDestroy()
		{
			if (IsDone() == false)
				return false;

			if (RefCount <= 0 && _providers.Count == 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 是否完毕（无论成功失败）
		/// </summary>
		public bool IsDone()
		{
			if (CheckFileLoadDone())
				return CheckProvidersDone();
			else
				return false;
		}

		/// <summary>
		/// 文件加载是否完毕
		/// </summary>
		public bool CheckFileLoadDone()
		{
			return States == ELoaderStates.Success || States == ELoaderStates.Fail;
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public abstract void WaitForAsyncComplete();

		#region Asset Provider
		private readonly List<IAssetProvider> _providers = new List<IAssetProvider>();

		/// <summary>
		/// 调试接口
		/// </summary>
		internal List<IAssetProvider> GetProviders()
		{
			return _providers;
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="sceneName">场景名称</param>
		public AssetOperationHandle LoadSceneAsync(string sceneName, SceneInstanceParam instanceParam)
		{
			IAssetProvider provider = TryGetProvider(sceneName);
			if (provider == null)
			{
				IsSceneLoader = true;
				provider = new AssetSceneProvider(this, sceneName, instanceParam);
				_providers.Add(provider);
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetName">资源名称</param>
		/// <param name="assetType">资源类型</param>
		/// <param name="syncLoadMode">同步加载模式</param>
		public AssetOperationHandle LoadAssetAsync(string assetName, System.Type assetType, bool syncLoadMode)
		{
			IAssetProvider provider = TryGetProvider(assetName);
			if (provider == null)
			{
				if (this is AssetBundleLoader)
					provider = new AssetBundleProvider(this, assetName, assetType);
				else if (this is AssetDatabaseLoader)
					provider = new AssetDatabaseProvider(this, assetName, assetType);
				else
					throw new NotImplementedException($"{this.GetType()}");
				_providers.Add(provider);
			}

			// 异步转同步
			if (syncLoadMode)
			{
				provider.SetSyncLoadMode();
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}

		/// <summary>
		/// 异步加载所有子资源对象
		/// </summary>
		/// <param name="assetName">资源名称</param>
		/// <param name="assetType">资源类型</param>、
		/// <param name="syncLoadMode">同步加载模式</param>
		public AssetOperationHandle LoadSubAssetsAsync(string assetName, System.Type assetType, bool syncLoadMode)
		{
			IAssetProvider provider = TryGetProvider(assetName);
			if (provider == null)
			{
				if (this is AssetBundleLoader)
					provider = new AssetBundleSubProvider(this, assetName, assetType);
				else if (this is AssetDatabaseLoader)
					provider = new AssetDatabaseSubProvider(this, assetName, assetType);
				else
					throw new NotImplementedException($"{this.GetType()}");
				_providers.Add(provider);
			}

			// 异步转同步
			if (syncLoadMode)
			{
				provider.SetSyncLoadMode();
			}

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}


		/// <summary>
		/// 检测所有的资源提供者是否完毕
		/// </summary>
		private bool CheckProvidersDone()
		{
			for (int i = 0; i < _providers.Count; i++)
			{
				var provider = _providers[i];
				if (provider.IsDone == false)
					return false;
			}
			return true;
		}

		/// <summary>
		/// 轮询更新所有的资源提供者
		/// </summary>
		private void UpdateProviders()
		{
			for (int i = _providers.Count - 1; i >= 0; i--)
			{
				var provider = _providers[i];
				provider.Update();

				// 检测是否可以销毁
				if (provider.CanDestroy())
				{
					provider.Destory();
					_providers.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// 获取一个资源提供者
		/// </summary>
		private IAssetProvider TryGetProvider(string assetName)
		{
			IAssetProvider provider = null;
			for (int i = 0; i < _providers.Count; i++)
			{
				IAssetProvider temp = _providers[i];
				if (temp.AssetName.Equals(assetName))
				{
					provider = temp;
					break;
				}
			}
			return provider;
		}
		#endregion
	}
}