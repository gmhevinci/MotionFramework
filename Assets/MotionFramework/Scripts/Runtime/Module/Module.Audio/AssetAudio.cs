//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using MotionFramework.Resource;
using YooAsset;

namespace MotionFramework.Audio
{
	/// <summary>
	/// 音频资源类
	/// </summary>
	internal class AssetAudio
	{
		private AssetOperationHandle _handle;
		private System.Action<AudioClip> _userCallback;
		private bool _isLoadAsset = false;

		/// <summary>
		/// 资源地址
		/// </summary>
		public string Location { private set; get; }

		/// <summary>
		/// 音频层级
		/// </summary>
		public EAudioLayer AudioLayer { private set; get; }

		/// <summary>
		/// 资源对象
		/// </summary>
		public AudioClip Clip { private set; get; }


		public AssetAudio(string location, EAudioLayer audioLayer)
		{
			Location = location;
			AudioLayer = audioLayer;
		}
		public void Load(System.Action<AudioClip> callback)
		{
			if (_isLoadAsset)
				return;

			_isLoadAsset = true;
			_userCallback = callback;
			_handle = ResourceManager.Instance.LoadAssetAsync<AudioClip>(Location);
			_handle.Completed += Handle_Completed;
		}
		public void UnLoad()
		{
			if(_isLoadAsset)
			{
				_isLoadAsset = false;
				_userCallback = null;
				_handle.Release();
			}
		}
		private void Handle_Completed(AssetOperationHandle obj)
		{
			Clip = _handle.AssetObject as AudioClip;
			_userCallback?.Invoke(Clip);	
		}
	}
}