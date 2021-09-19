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
	internal sealed class BundledSceneProvider : BundledProvider
	{
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
				if (DependBundles.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.CacheBundle == null)
				{
					States = EAssetStates.Fail;
					InvokeCompletion();
				}
				else
				{
					States = EAssetStates.Loading;
				}
			}

			// 2. 加载场景
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

			// 3. 检测加载结果
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

			// 卸载附加场景（异步方式卸载）
			if (_param.LoadMode == LoadSceneMode.Additive)
				SceneManager.UnloadSceneAsync(AssetName);
		}
		public override void WaitForAsyncComplete()
		{
			throw new System.Exception($"Unity scene is not support {nameof(WaitForAsyncComplete)}.");
		}
	}
}