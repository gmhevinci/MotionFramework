//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;
using System;

namespace MotionFramework.Resource
{
	internal abstract class AssetLoaderBase
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


		public AssetLoaderBase(AssetBundleInfo bundleInfo)
		{
			BundleInfo = bundleInfo;
			RefCount = 0;
			States = ELoaderStates.None;
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// 引用（引用计数递加）
		/// </summary>
		public virtual void Reference()
		{
			RefCount++;
		}

		/// <summary>
		/// 释放（引用计数递减）
		/// </summary>
		public virtual void Release()
		{
			RefCount--;
		}

		/// <summary>
		/// 销毁
		/// </summary>
		public virtual void Destroy(bool force)
		{
			IsDestroyed = true;
		}

		/// <summary>
		/// 是否完毕（无论成功失败）
		/// </summary>
		public bool IsDone()
		{
			if (States == ELoaderStates.Success || States == ELoaderStates.Fail)
				return CheckAllProviderIsDone();
			else
				return false;
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
		/// 强制同步加载资源
		/// </summary>
		public abstract void ForceSyncLoad();

		#region Asset Provider
		internal readonly List<IAssetProvider> _providers = new List<IAssetProvider>();

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
		/// <param name="param">附加参数</param>
		public AssetOperationHandle LoadAssetAsync(string assetName, System.Type assetType)
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

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}

		/// <summary>
		/// 异步加载所有子资源对象
		/// </summary>
		/// <param name="assetName">资源名称</param>
		/// <param name="assetType">资源类型</param>
		public AssetOperationHandle LoadSubAssetsAsync(string assetName, System.Type assetType)
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

			// 引用计数增加
			provider.Reference();
			return provider.Handle;
		}

		/// <summary>
		/// 获取失败的资源提供者总数
		/// </summary>
		public int GetFailedProviderCount()
		{
			int failedCount = 0;
			for (int i = 0; i < _providers.Count; i++)
			{
				var provider = _providers[i];
				if (provider.States == EAssetStates.Fail)
					failedCount++;
			}
			return failedCount;
		}

		/// <summary>
		/// 检测所有资源提供者是否完毕
		/// </summary>
		protected bool CheckAllProviderIsDone()
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
		/// 轮询更新所有资源提供者
		/// </summary>
		protected void UpdateAllProvider()
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

		// 获取一个资源提供者
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