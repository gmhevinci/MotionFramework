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
	public abstract class UIWindow : IEnumerator
	{
		protected readonly EventGroup _eventGroup = new EventGroup();
		private AssetReference _assetRef;
		private AssetOperationHandle _handle;
		private System.Action<UIWindow> _prepareCallback;

		/// <summary>
		/// 窗口名称
		/// </summary>
		public string WindowName { private set; get; }

		/// <summary>
		/// 窗口层级
		/// </summary>
		public int WindowLayer { private set; get; }

		/// <summary>
		/// 是否为常驻窗口
		/// </summary>
		public bool DontDestroy { private set; get; }

		/// <summary>
		/// 是否是全屏窗口
		/// </summary>
		public bool FullScreen { private set; get; }

		/// <summary>
		/// 实例化对象
		/// </summary>
		public GameObject Go { private set; get; }

		/// <summary>
		/// 自定义数据
		/// </summary>
		public System.Object UserData { private set; get; }

		/// <summary>
		/// 是否加载完毕
		/// </summary>
		public bool IsDone { get { return _handle.IsDone; } }

		/// <summary>
		/// 是否准备完毕
		/// </summary>
		public bool IsPrepare { get { return Go != null; } }

		/// <summary>
		/// 窗口是否打开
		/// </summary>
		public bool IsOpen { private set; get; } = false;

		/// <summary>
		/// 窗口深度值
		/// </summary>
		public abstract int Depth { get; set; }

		/// <summary>
		/// 窗口可见性
		/// </summary>
		public abstract bool Visible { get; set; }


		public void Init(string name, int layer, bool dontDestroy, bool fullScreen)
		{
			WindowName = name;
			WindowLayer = layer;
			DontDestroy = dontDestroy;
			FullScreen = fullScreen;
		}
		public abstract void OnCreate();
		public abstract void OnDestroy();
		public abstract void OnRefresh();
		public abstract void OnUpdate();
		public virtual void OnSortDepth() { }
		public virtual void OnSetVisible() { }

		internal void InternalOpen(System.Object userData)
		{
			UserData = userData;
			IsOpen = true;
			if (Go != null && Go.activeSelf == false)
				Go.SetActive(true);
		}
		internal void InternalClose()
		{
			IsOpen = false;
			if (Go != null && Go.activeSelf)
				Go.SetActive(false);
		}
		internal void InternalLoad(string location, System.Action<UIWindow> prepareCallback)
		{
			if (IsPrepare)
				prepareCallback?.Invoke(this);

			if (_assetRef == null)
			{
				_prepareCallback = prepareCallback;
				_assetRef = new AssetReference(location);
				_handle = _assetRef.LoadAssetAsync<GameObject>();
				_handle.Completed += Handle_Completed;
			}
		}
		internal void InternalDestroy()
		{
			// 注销回调函数
			_prepareCallback = null;

			// 销毁面板对象
			if (Go != null)
			{
				OnDestroy();
				GameObject.Destroy(Go);
				Go = null;
			}

			// 卸载面板资源
			if (_assetRef != null)
			{
				_assetRef.Release();
				_assetRef = null;
			}

			// 移除所有缓存的事件监听
			_eventGroup.RemoveAllListener();
		}
		internal void InternalUpdate()
		{
			OnUpdate();
		}
		private void Handle_Completed(AssetOperationHandle obj)
		{
			if (_handle.AssetObject == null)
				return;

			// 实例化对象
			Go = _handle.InstantiateObject;

			// 设置UI桌面
			GameObject uiDesktop = WindowManager.Instance.Root.UIDesktop;
			Go.transform.SetParent(uiDesktop.transform, false);

			// 调用重载函数
			OnAssetLoad(Go);
			OnCreate();

			// 最后设置是否激活
			Go.SetActive(IsOpen);

			// 通知UI管理器
			_prepareCallback?.Invoke(this);
		}
		protected abstract void OnAssetLoad(GameObject go);

		#region 异步相关
		bool IEnumerator.MoveNext()
		{
			return !IsDone;
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