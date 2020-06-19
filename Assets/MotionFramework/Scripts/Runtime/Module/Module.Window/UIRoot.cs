//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Resource;
using MotionFramework.Event;

namespace MotionFramework.Window
{
	public abstract class UIRoot : IEnumerator
	{
		private AssetReference _assetRef;
		private AssetOperationHandle _handle;

		/// <summary>
		/// GameObject对象
		/// </summary>
		public GameObject Go { private set; get; }

		/// <summary>
		/// UI桌面
		/// </summary>
		public virtual GameObject UIDesktop { protected set; get; }

		/// <summary>
		/// UI相机
		/// </summary>
		public virtual Camera UICamera { protected set; get; }

		internal void InternalLoad(string location)
		{
			if (_assetRef == null)
			{
				_assetRef = new AssetReference(location);
				_handle = _assetRef.LoadAssetAsync<GameObject>();
				_handle.Completed += Handle_Completed;
			}
		}
		internal void InternalDestroy()
		{
			if (Go != null)
			{
				GameObject.Destroy(Go);
				Go = null;
			}

			if (_assetRef != null)
			{
				_assetRef.Release();
				_assetRef = null;
			}
		}
		private void Handle_Completed(AssetOperationHandle obj)
		{
			if (_handle.AssetObject == null)
				return;

			Go = _handle.InstantiateObject;
			GameObject.DontDestroyOnLoad(Go);
			OnAssetLoad(Go);
		}
		protected abstract void OnAssetLoad(GameObject go);

		#region 异步相关
		bool IEnumerator.MoveNext()
		{
			return !_handle.IsDone;
		}
		void IEnumerator.Reset()
		{
		}
		object IEnumerator.Current
		{
			get { return null; }
		}
		#endregion
	}
}