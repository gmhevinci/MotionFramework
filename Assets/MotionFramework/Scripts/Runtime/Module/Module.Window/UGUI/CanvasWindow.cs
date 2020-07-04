//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using UnityEngine;
using UnityEngine.UI;
using MotionFramework.Window;

public abstract class CanvasWindow : UIWindow
{
	public const int WINDOW_HIDE_LAYER = 2; // Ignore Raycast
	public const int WINDOW_SHOW_LAYER = 5; // UI

	private UIManifest _manifest;
	private Canvas _canvas;
	private Canvas[] _childCanvas;
	private GraphicRaycaster _raycaster;
	private GraphicRaycaster[] _childRaycaster;

	/// <summary>
	/// 窗口深度值
	/// </summary>
	public override int Depth
	{
		get
		{
			if (_canvas != null)
				return _canvas.sortingOrder;
			else
				return 0;
		}

		set
		{
			if (_canvas != null)
			{
				if (_canvas.sortingOrder == value)
					return;

				// 设置父类
				_canvas.sortingOrder = value;

				// 设置子类
				int depth = value;
				for (int i = 0; i < _childCanvas.Length; i++)
				{
					var canvas = _childCanvas[i];
					if (canvas != _canvas)
					{
						depth += 5; //注意递增值
						canvas.sortingOrder = depth;
					}
				}

				// 虚函数
				OnSortDepth(value);
			}
		}
	}

	/// <summary>
	/// 窗口是否可见
	/// </summary>
	public override bool Visible
	{
		get
		{
			if (_canvas != null && _raycaster != null)
				return _canvas.gameObject.layer == WINDOW_SHOW_LAYER;
			else
				return false;
		}
		set
		{
			if (_canvas != null && _raycaster != null)
			{
				int setLayer = value ? WINDOW_SHOW_LAYER : WINDOW_HIDE_LAYER;
				if (_canvas.gameObject.layer == setLayer)
					return;

				// 显示设置
				_canvas.gameObject.layer = setLayer;
				for (int i = 0; i < _childCanvas.Length; i++)
				{
					_childCanvas[i].gameObject.layer = setLayer;
				}

				// 交互设置
				_raycaster.enabled = value;
				for (int i = 0; i < _childRaycaster.Length; i++)
				{
					_childRaycaster[i].enabled = value;
				}

				// 虚函数
				OnSetVisible(value);
			}
		}
	}

	public CanvasWindow()
	{
	}
	protected override void OnAssetLoad(GameObject go)
	{
		// 获取组件
		_manifest = go.GetComponent<UIManifest>();
		if (_manifest == null)
			throw new Exception($"Not found {nameof(UIManifest)} in panel {WindowName}");

		// 获取组件
		_canvas = go.GetComponent<Canvas>();
		if (_canvas == null)
			throw new Exception($"Not found {nameof(Canvas)} in panel {WindowName}");
		_canvas.overrideSorting = true;
		_canvas.sortingOrder = 0;

		// 获取组件
		_raycaster = go.GetComponent<GraphicRaycaster>();
		_childCanvas = go.GetComponentsInChildren<Canvas>(true);
		_childRaycaster = go.GetComponentsInChildren<GraphicRaycaster>(true);
	}

	#region UI组件相关
	/// <summary>
	/// 获取窗口里缓存的元素对象
	/// </summary>
	/// <param name="path">对象路径</param>
	protected Transform GetUIElement(string path)
	{
		if (_manifest == null)
			return null;
		return _manifest.GetUIElement(path);
	}

	/// <summary>
	/// 获取窗口里缓存的组件对象
	/// </summary>
	/// <typeparam name="T">组件类型</typeparam>
	/// <param name="path">组件路径</param>
	protected T GetUIComponent<T>(string path) where T : UnityEngine.Component
	{
		if (_manifest == null)
			return null;
		return _manifest.GetUIComponent<T>(path);
	}

	/// <summary>
	/// 获取窗口里缓存的组件对象
	/// </summary>
	/// <param name="path">组件路径</param>
	/// <param name="typeName">组件类型名称</param>
	protected UnityEngine.Component GetUIComponent(string path, string typeName)
	{
		if (_manifest == null)
			return null;
		return _manifest.GetUIComponent(path, typeName);
	}

	/// <summary>
	/// 监听按钮点击事件
	/// </summary>
	protected void AddButtonListener(string path, UnityEngine.Events.UnityAction call)
	{
		Button btn = GetUIComponent<Button>(path);
		if (btn != null)
			btn.onClick.AddListener(call);
	}
	#endregion
}