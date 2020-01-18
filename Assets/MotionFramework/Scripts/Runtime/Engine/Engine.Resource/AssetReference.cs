//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.IO;

namespace MotionFramework.Resource
{
	public class AssetReference
	{
		/// <summary>
		/// 资源定位地址
		/// 注意：资源在工程内以AssetSystem.AssetRootPath为根路径的相对路径
		/// </summary>
		public string Location { private set; get; }

		/// <summary>
		/// 加载器
		/// </summary>
		private AssetLoaderBase _cacheLoader;


		public AssetReference(string location)
		{
			Location = location;
		}

		/// <summary>
		/// 释放资源
		/// </summary>
		public void Release()
		{
			if (_cacheLoader != null)
			{
				_cacheLoader.Release();
				_cacheLoader = null;
			}
			Location = string.Empty;
		}

		/// <summary>
		/// 异步加载主资源对象
		/// </summary>
		public AssetOperationHandle LoadAssetAsync<TObject>()
		{
			string assetName = Path.GetFileNameWithoutExtension(Location);
			return LoadInternal(assetName, typeof(TObject), null);
		}

		/// <summary>
		/// 异步加载主资源对象
		/// </summary>
		public AssetOperationHandle LoadAssetAsync<TObject>(IAssetParam param)
		{
			string assetName = Path.GetFileNameWithoutExtension(Location);
			return LoadInternal(assetName, typeof(TObject), param);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetName">资源对象名称</param>
		public AssetOperationHandle LoadAssetAsync<TObject>(string assetName)
		{
			return LoadInternal(assetName, typeof(TObject), null);
		}

		/// <summary>
		/// 异步加载资源对象
		/// <param name="assetName">资源对象名称</param>
		/// </summary>
		public AssetOperationHandle LoadAssetAsync<TObject>(string assetName, IAssetParam param)
		{
			return LoadInternal(assetName, typeof(TObject), param);
		}

		private AssetOperationHandle LoadInternal(string assetName, System.Type assetType, IAssetParam param)
		{
			if (_cacheLoader == null)
				_cacheLoader = AssetSystem.CreateLoader(Location);
			return _cacheLoader.LoadAssetAsync(assetName, assetType, param);
		}
	}
}