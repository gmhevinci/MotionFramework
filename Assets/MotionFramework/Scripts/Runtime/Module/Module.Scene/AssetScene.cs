//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine.SceneManagement;
using MotionFramework.Resource;
using YooAsset;

namespace MotionFramework.Scene
{
	internal class AssetScene
	{
		private SceneOperationHandle _handle;
		private System.Action<SceneOperationHandle> _finishCallback;
		private System.Action<int> _progressCallback;
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
				if (_handle == null)
					return 0;
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
				if (_handle == null)
					return false;
				return _handle.IsDone;
			}
		}


		public AssetScene(string location)
		{
			Location = location;
		}
		public void Load(bool isAdditive, bool activeOnLoad, System.Action<SceneOperationHandle> finishCallback, System.Action<int> progressCallbcak)
		{
			if (_handle != null)
				return;

			var _sceneMode = isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single;

			MotionLog.Log($"Begin to load scene : {Location}");
			_finishCallback = finishCallback;
			_progressCallback = progressCallbcak;
			_handle = ResourceManager.Instance.LoadSceneAsync(Location, _sceneMode, activeOnLoad);
			_handle.Completed += Handle_Completed;
		}
		public void UnLoad()
		{
			if (_handle != null)
			{
				MotionLog.Log($"Begin to unload scene : {Location}");
				_finishCallback = null;
				_progressCallback = null;
				_lastProgressValue = 0;

				// 异步卸载场景
				_handle.UnloadAsync();
				_handle = null;
			}
		}
		public void Update()
		{
			if (_handle != null)
			{
				if (_lastProgressValue != Progress)
				{
					_lastProgressValue = Progress;
					_progressCallback?.Invoke(_lastProgressValue);
				}
			}
		}

		// 资源回调
		private void Handle_Completed(SceneOperationHandle handle)
		{
			_finishCallback?.Invoke(_handle);
		}
	}
}