//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using UnityEngine;
using MotionFramework.Resource;

namespace MotionFramework.Scene
{
	internal class AssetScene
	{
		private AssetReference _assetRef;
		private AssetOperationHandle _handle;
		private System.Action<SceneInstance> _userCallback;

		/// <summary>
		/// 场景地址
		/// </summary>
		public string Location
		{
			get
			{
				return _assetRef.Location;
			}
		}

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
			_assetRef = new AssetReference(location);
		}
		public void Load(bool isAdditive, bool activeOnLoad, System.Action<SceneInstance> callback)
		{
			if (_userCallback != null)
				return;

			// 场景加载参数
			SceneInstanceParam param = new SceneInstanceParam();
			param.IsAdditive = isAdditive;
			param.ActivateOnLoad = activeOnLoad;

			_userCallback = callback;	
			_handle = _assetRef.LoadAssetAsync<SceneInstance>(param);
			_handle.Completed += Handle_Completed;
		}
		public void UnLoad()
		{
			if (_assetRef != null)
			{
				_assetRef.Release();
				_assetRef = null;
			}
			_userCallback = null;
		}

		// 资源回调
		private void Handle_Completed(AssetOperationHandle obj)
		{
			_userCallback?.Invoke(_handle.AssetObject as SceneInstance);
		}
	}
}