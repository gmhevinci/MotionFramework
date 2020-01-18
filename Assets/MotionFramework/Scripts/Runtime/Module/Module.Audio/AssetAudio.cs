//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using MotionFramework.Resource;

namespace MotionFramework.Audio
{
	/// <summary>
	/// 音频资源类
	/// </summary>
	internal class AssetAudio
	{
		private AssetReference _assetRef;
		private AssetOperationHandle _handle;
		private System.Action<AudioClip> _userCallback;

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
			AudioLayer = audioLayer;
			_assetRef = new AssetReference(location);
		}
		public void Load(System.Action<AudioClip> callback)
		{
			if (_userCallback != null)
				return;

			_userCallback = callback;
			_handle = _assetRef.LoadAssetAsync<AudioClip>();
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
		private void Handle_Completed(AssetOperationHandle obj)
		{
			Clip = _handle.AssetObject as AudioClip;
			_userCallback?.Invoke(Clip);	
		}
	}
}