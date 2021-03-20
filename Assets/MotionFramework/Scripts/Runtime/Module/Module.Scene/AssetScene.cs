//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Resource;

namespace MotionFramework.Scene
{
	internal class AssetScene
	{
		private AssetOperationHandle _handle;
		private System.Action<SceneInstance> _userCallback;
		private bool _isLoadScene = false;

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
				return Progress == 100;
			}
		}


		public AssetScene(string location)
		{
			Location = location;
		}
		public void Load(bool isAdditive, bool activeOnLoad, System.Action<SceneInstance> callback)
		{
			if (_isLoadScene)
				return;

			// 场景加载参数
			SceneInstanceParam param = new SceneInstanceParam
			{
				IsAdditive = isAdditive,
				ActivateOnLoad = activeOnLoad
			};

			MotionLog.Log($"Begin to load scene : {Location}");
			_isLoadScene = true;
			_userCallback = callback;	
			_handle = ResourceManager.Instance.LoadSceneAsync(Location, param);
			_handle.Completed += Handle_Completed;
		}
		public void UnLoad()
		{
			if (_isLoadScene)
			{
				_isLoadScene = false;
				_userCallback = null;
				_handle.Release();
			}
		}

		// 资源回调
		private void Handle_Completed(AssetOperationHandle obj)
		{
			_userCallback?.Invoke(_handle.AssetInstance as SceneInstance);
		}
	}
}