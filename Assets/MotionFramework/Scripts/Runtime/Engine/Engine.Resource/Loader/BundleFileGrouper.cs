//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Resource
{
	internal class BundleFileGrouper
	{
		/// <summary>
		/// 所属的资源包加载器
		/// </summary>
		private readonly BundleFileLoader _ownerLoader;

		/// <summary>
		/// 依赖的资源包加载器列表
		/// </summary>
		private readonly List<BundleFileLoader> _dependLoaders;


		/// <summary>
		/// 所属的资源包对象
		/// </summary>
		public AssetBundle OwnerAssetBundle
		{
			get
			{
				return _ownerLoader.CacheBundle;
			}
		}

		/// <summary>
		/// 所属的资源包信息
		/// </summary>
		public AssetBundleInfo OwnerBundleInfo
		{
			get
			{
				return _ownerLoader.BundleInfo;
			}
		}


		public BundleFileGrouper(string assetPath)
		{
			_ownerLoader = CreateOwnerLoader(assetPath);
			_dependLoaders = CreateDependLoader(assetPath);
		}

		/// <summary>
		/// 设置为所属资源包为场景资源包
		/// </summary>
		public void SetSceneBundle()
		{
			_ownerLoader.SetSceneBundle();
		}

		/// <summary>
		/// 是否已经完成（无论成功或失败）
		/// </summary>
		public bool IsDone()
		{
			foreach (var dpLoader in _dependLoaders)
			{
				if (dpLoader.IsDone() == false)
					return false;
			}

			if (_ownerLoader.IsDone() == false)
				return false;

			return true;
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public void WaitForAsyncComplete()
		{
			foreach (var dpLoader in _dependLoaders)
			{
				if(dpLoader.IsDone() == false)
					dpLoader.WaitForAsyncComplete();
			}

			if (_ownerLoader.IsDone() == false)
				_ownerLoader.WaitForAsyncComplete();
		}

		/// <summary>
		/// 增加引用计数
		/// </summary>
		public void Reference()
		{
			_ownerLoader.Reference();
			foreach (var dpLoader in _dependLoaders)
			{
				dpLoader.Reference();
			}
		}

		/// <summary>
		/// 减少引用计数
		/// </summary>
		public void Release()
		{
			_ownerLoader.Release();
			foreach (var dpLoader in _dependLoaders)
			{
				dpLoader.Release();
			}
		}

		private BundleFileLoader CreateOwnerLoader(string assetPath)
		{
			string bundleName = AssetSystem.BundleServices.GetAssetBundleName(assetPath);
			AssetBundleInfo bundleInfo = AssetSystem.BundleServices.GetAssetBundleInfo(bundleName);
			return AssetSystem.GetOrCreateBundleFileLoader(bundleInfo);
		}
		private List<BundleFileLoader> CreateDependLoader(string assetPath)
		{
			List<BundleFileLoader> result = new List<BundleFileLoader>();
			string[] depends = AssetSystem.BundleServices.GetAllDependencies(assetPath);
			if (depends != null)
			{
				foreach (var dependBundleName in depends)
				{
					AssetBundleInfo dependBundleInfo = AssetSystem.BundleServices.GetAssetBundleInfo(dependBundleName);
					BundleFileLoader dependLoader = AssetSystem.GetOrCreateBundleFileLoader(dependBundleInfo);
					result.Add(dependLoader);
				}
			}
			return result;
		}
	}
}