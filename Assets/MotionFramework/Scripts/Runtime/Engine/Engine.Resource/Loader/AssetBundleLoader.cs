//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Resource
{
	internal sealed class AssetBundleLoader : AssetLoaderBase
	{
		private readonly List<AssetLoaderBase> _depends = new List<AssetLoaderBase>(10);
		private string _manifestPath = string.Empty;
		private AssetBundleCreateRequest _cacheRequest;
		internal AssetBundle CacheBundle { private set; get; }

		public AssetBundleLoader(string loadPath, string manifestPath)
			: base(loadPath)
		{
			_manifestPath = manifestPath;
		}
		public override void Update()
		{
			// 如果资源文件加载完毕
			if (States == ELoaderStates.Success || States == ELoaderStates.Fail)
			{
				UpdateAllProvider();
				return;
			}

			if (States == ELoaderStates.None)
			{
				States = ELoaderStates.LoadDepends;
			}

			// 1. 加载所有依赖项
			if (States == ELoaderStates.LoadDepends)
			{
				string[] dependencies = AssetSystem.BundleServices.GetDirectDependencies(_manifestPath);
				if (dependencies.Length > 0)
				{
					foreach (string dpManifestPath in dependencies)
					{
						string dpLoadPath = AssetSystem.BundleServices.GetAssetBundleLoadPath(dpManifestPath);
						AssetLoaderBase dpLoader = AssetSystem.CreateLoaderInternal(dpLoadPath, dpManifestPath);
						_depends.Add(dpLoader);
					}
				}
				States = ELoaderStates.CheckDepends;
			}

			// 2. 检测所有依赖完成状态
			if (States == ELoaderStates.CheckDepends)
			{
				foreach (var dpLoader in _depends)
				{
					if (dpLoader.IsDone() == false)
						return;
				}
				States = ELoaderStates.LoadFile;
			}

			// 3. 加载AssetBundle
			if (States == ELoaderStates.LoadFile)
			{
#if UNITY_EDITOR
				// 注意：Unity2017.4编辑器模式下，如果AssetBundle文件不存在会导致编辑器崩溃，这里做了预判。
				if (System.IO.File.Exists(LoadPath) == false)
				{
					MotionLog.Log(ELogLevel.Warning, $"Not found assetBundle file : {LoadPath}");
					States = ELoaderStates.Fail;
					return;
				}
#endif

				// Load assetBundle file
				if( AssetSystem.DecryptServices != null)
				{
					if(AssetSystem.DecryptServices.DecryptType == EDecryptMethod.GetDecryptOffset)
					{
						ulong offset = AssetSystem.DecryptServices.GetDecryptOffset(LoadPath);
						_cacheRequest = AssetBundle.LoadFromFileAsync(LoadPath, 0, offset);
					}
					else if(AssetSystem.DecryptServices.DecryptType == EDecryptMethod.GetDecryptBinary)
					{
						byte[] binary = AssetSystem.DecryptServices.GetDecryptBinary(LoadPath);
						_cacheRequest = AssetBundle.LoadFromMemoryAsync(binary);
					}
					else
					{
						throw new NotImplementedException($"{AssetSystem.DecryptServices.DecryptType}");
					}
				}
				else
				{
					_cacheRequest = AssetBundle.LoadFromFileAsync(LoadPath);
				}
				States = ELoaderStates.CheckFile;		
			}

			// 4. 检测AssetBundle加载结果
			if (States == ELoaderStates.CheckFile)
			{
				if (_cacheRequest.isDone == false)
					return;
				CacheBundle = _cacheRequest.assetBundle;

				// Check error
				if (CacheBundle == null)
				{
					MotionLog.Log(ELogLevel.Warning, $"Failed to load assetBundle file : {LoadPath}");
					States = ELoaderStates.Fail;
				}
				else
				{
					States = ELoaderStates.Success;
				}
			}
		}
		public override void Reference()
		{
			base.Reference();

			// 同时引用一遍所有依赖资源
			for (int i = 0; i < _depends.Count; i++)
			{
				_depends[i].Reference();
			}
		}
		public override void Release()
		{
			base.Release();

			// 同时释放一遍所有依赖资源
			for (int i = 0; i < _depends.Count; i++)
			{
				_depends[i].Release();
			}
		}
		public override void Destroy(bool force)
		{
			base.Destroy(force);

			// Check fatal
			if (RefCount > 0)
				throw new Exception($"Bundle file loader ref is not zero : {LoadPath}");
			if (IsDone() == false)
				throw new Exception($"Bundle file loader is not done : {LoadPath}");

			// 卸载AssetBundle
			if (CacheBundle != null)
			{
				CacheBundle.Unload(force);
				CacheBundle = null;
			}

			_depends.Clear();
		}
		public override bool IsDone()
		{
			if (base.IsDone() == false)
				return false;

			return CheckAllProviderIsDone();
		}
	}
}