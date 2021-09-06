//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MotionFramework.Resource
{
	internal sealed class BundledSceneProvider : AssetProviderBase
	{
		private BundleFileGrouper _bundleGrouper;
		private SceneInstanceParam _param;
		private AsyncOperation _asyncOp;
		public override float Progress
		{
			get
			{
				if (_asyncOp == null)
					return 0;
				return _asyncOp.progress;
			}
		}

		public BundledSceneProvider(string scenePath, SceneInstanceParam param)
			: base(scenePath, null)
		{
			_param = param;
			_bundleGrouper = new BundleFileGrouper(scenePath);
			_bundleGrouper.Reference();
		}
		public override void Update()
		{
			if (IsDone)
				return;

			if (States == EAssetStates.None)
			{
				States = EAssetStates.CheckBundle;
			}

			// 1. 检测资源包
			if (States == EAssetStates.CheckBundle)
			{
				if (_bundleGrouper.IsDone() == false)
					return;

				if (_bundleGrouper.OwnerAssetBundle == null)
				{
					States = EAssetStates.Fail;
					InvokeCompletion();
				}
				else
				{
					States = EAssetStates.Loading;
				}
			}

			// 1. 加载资源对象
			if (States == EAssetStates.Loading)
			{
				_asyncOp = SceneManager.LoadSceneAsync(AssetName, _param.LoadMode);		
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = _param.ActivateOnLoad;
					States = EAssetStates.Checking;
				}
				else
				{
					MotionLog.Warning($"Failed to load scene : {AssetName}");
					States = EAssetStates.Fail;
					InvokeCompletion();
				}
			}

			// 2. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				if (_asyncOp.isDone || (_param.ActivateOnLoad == false && _asyncOp.progress == 0.9f))
				{
					SceneInstance instance = new SceneInstance(_asyncOp);
					instance.Scene = SceneManager.GetSceneByName(AssetName);
					AssetInstance = instance;
					States = EAssetStates.Success;
					InvokeCompletion();
				}
			}
		}
		public override void Destory()
		{
			base.Destory();

			// 释放资源包
			if (_bundleGrouper != null)
			{
				_bundleGrouper.Release();
				_bundleGrouper = null;
			}

			// 卸载附加场景
			if (_param.LoadMode == LoadSceneMode.Additive)
				SceneManager.UnloadSceneAsync(AssetName);
		}
		public override void WaitForAsyncComplete()
		{
			throw new System.Exception($"Unity scene is not support {nameof(WaitForAsyncComplete)}.");
		}

		/// <summary>
		/// 获取资源包的调试信息列表
		/// </summary>
		internal void GetBundleDebugInfos(List<BundleDebugInfo> output)
		{
			_bundleGrouper.GetBundleDebugInfos(output);
		}
	}
}