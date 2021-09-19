//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MotionFramework.Resource
{
	internal sealed class DatabaseSceneProvider : AssetProviderBase
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

		public DatabaseSceneProvider(string scenePath, SceneInstanceParam param)
			: base(scenePath, null)
		{
			_param = param;
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (States == EAssetStates.None)
			{
				States = EAssetStates.Loading;
			}

			// 1. 加载资源对象
			if (States == EAssetStates.Loading)
			{
				LoadSceneParameters loadSceneParameters = new LoadSceneParameters();
				loadSceneParameters.loadSceneMode = _param.LoadMode;			
				_asyncOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(AssetPath, loadSceneParameters);
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
#endif
		}
		public override void Destory()
		{
#if UNITY_EDITOR
			base.Destory();

			// 卸载附加场景（异步方式卸载）
			if (_param.LoadMode == LoadSceneMode.Additive)
				SceneManager.UnloadSceneAsync(AssetName);
#endif
		}
		public override void WaitForAsyncComplete()
		{
			throw new System.Exception($"Unity scene is not support {nameof(WaitForAsyncComplete)}.");
		}
	}
}