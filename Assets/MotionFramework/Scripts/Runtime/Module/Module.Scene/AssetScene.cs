//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Resource;
using YooAsset;

namespace MotionFramework.Scene
{
	internal class AssetScene
	{
		private AssetOperationHandle _handle;
		private System.Action<SceneInstance> _finishCallback;
		private System.Action<int> _progressCallback;
		private bool _isLoadScene = false;
		private int _lastProgressValue = 0;

		/// <summary>
		/// 场景地址
		/// </summary>
		public string Location { private set; get; }

		/// <summary>
		/// 场景加载进度（0-100）
		/// </summary>
		public int Progress
		{
			get
			{
				return (int)(_handle.Progress * 100f);
			}
		}

		/// <summary>
		/// 场景是否加载完毕
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _handle.IsDone;
			}
		}


		public AssetScene(string location)
		{
			Location = location;
		}
		public void Load(bool isAdditive, bool activeOnLoad, System.Action<SceneInstance> finishCallback, System.Action<int> progressCallbcak)
		{
			if (_isLoadScene)
				return;

			// 场景加载参数
			SceneInstanceParam param = new SceneInstanceParam
			{
				LoadMode = isAdditive ? UnityEngine.SceneManagement.LoadSceneMode.Additive : UnityEngine.SceneManagement.LoadSceneMode.Single,
				ActivateOnLoad = activeOnLoad
			};

			MotionLog.Log($"Begin to load scene : {Location}");
			_isLoadScene = true;
			_finishCallback = finishCallback;
			_progressCallback = progressCallbcak;
			_handle = ResourceManager.Instance.LoadSceneAsync(Location, param);
			_handle.Completed += Handle_Completed;
		}
		public void UnLoad()
		{
			if (_isLoadScene)
			{
				MotionLog.Log($"Begin to unload scene : {Location}");
				_isLoadScene = false;
				_finishCallback = null;
				_progressCallback = null;
				_lastProgressValue = 0;
				_handle.Release();
			}
		}
		public void Update()
		{
			if (_isLoadScene)
			{
				if (_lastProgressValue != Progress)
				{
					_lastProgressValue = Progress;
					_progressCallback?.Invoke(_lastProgressValue);
				}
			}
		}

		// 资源回调
		private void Handle_Completed(AssetOperationHandle obj)
		{
			_finishCallback?.Invoke(_handle.AssetInstance as SceneInstance);
		}
	}
}