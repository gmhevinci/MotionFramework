//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Window
{
	public class UIManager : ModuleSingleton<UIManager>, IModule
	{
		private readonly List<UIWindow> _stack = new List<UIWindow>(100);

		void IModule.OnCreate(object createParam)
		{
		}
		void IModule.OnUpdate()
		{
			int count = _stack.Count;
			for (int i = 0; i < _stack.Count; i++)
			{
				if (_stack.Count != count)
					break;
				UIWindow window = _stack[i];
				if (window.IsPrepare && window.IsOpen)
					window.InternalUpdate();
			}
		}
		void IModule.OnGUI()
		{
		}

		/// <summary>
		/// UI根节点
		/// </summary>
		public UIRoot Root { private set; get; }

		/// <summary>
		/// 是否有窗口正在加载
		/// </summary>
		public bool IsLoading()
		{
			for (int i = 0; i < _stack.Count; i++)
			{
				UIWindow window = _stack[i];
				if (window.IsDone == false)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 查询顶端窗口
		/// </summary>
		public bool IsTop(string name, int layer)
		{
			UIWindow lastOne = null;
			for (int i = 0; i < _stack.Count; i++)
			{
				if (_stack[i].WindowLayer == layer)
					lastOne = _stack[i];
			}

			if (lastOne == null)
				return false;

			return lastOne.WindowName == name;
		}

		/// <summary>
		/// 创建UIRoot
		/// </summary>
		public UIRoot CreateUIRoot<T>(string location) where T : UIRoot
		{
			return CreateUIRoot(typeof(T), location);
		}
		public UIRoot CreateUIRoot(Type type, string location)
		{
			if (Root != null)
				throw new Exception("UIRoot has been created.");

			Root = Activator.CreateInstance(type) as UIRoot;
			if (Root == null)
				throw new Exception($"UIRoot {type.FullName} create instance failed.");
			Root.InternalLoad(location);
			return Root;
		}

		/// <summary>
		/// 预加载窗口
		/// </summary>
		public UIWindow PreloadWindow<T>(string location) where T : UIWindow
		{
			return PreloadWindow(typeof(T), location);
		}
		public UIWindow PreloadWindow(Type type, string location)
		{
			string windowName = type.Name;

			// 如果窗口已经存在
			if (IsContains(windowName))
				return GetWindow(windowName);

			UIWindow window = CreateInstance(type);
			Push(window); // 首次压入
			window.InternalClose();
			window.InternalLoad(location, OnWindowPrepare);
			return window;
		}

		/// <summary>
		/// 打开窗口
		/// </summary>
		/// <param name="location">资源路径</param>
		/// <param name="userData">用户数据</param>
		public UIWindow OpenWindow<T>(string location, System.Object userData = null) where T : UIWindow
		{
			return OpenWindow(typeof(T), location, userData);
		}
		public UIWindow OpenWindow(Type type, string location, System.Object userData = null)
		{
			string windowName = type.Name;

			// 如果窗口已经存在
			UIWindow window;
			if (IsContains(windowName))
			{
				window = GetWindow(windowName);
				Pop(window); //弹出旧窗口
			}
			else
			{
				window = CreateInstance(type);
			}

			Push(window); // 首次压入或重新压入
			window.InternalOpen(userData);
			window.InternalLoad(location, OnWindowPrepare);
			return window;
		}

		/// <summary>
		/// 关闭窗口
		/// </summary>
		public void CloseWindow<T>() where T : UIWindow
		{
			CloseWindow(typeof(T));
		}
		public void CloseWindow(Type type)
		{
			string windowName = type.Name;
			UIWindow window = GetWindow(windowName);
			if (window == null)
				return;

			if (window.DontDestroy)
			{
				window.InternalClose();
			}
			else
			{
				window.InternalDestroy();
				Pop(window);
				OnSortWindowDepth(window.WindowLayer);
				OnSetWindowVisible(window.WindowLayer);
			}
		}

		/// <summary>
		/// 关闭所有窗口
		/// 注意：常驻窗口除外
		/// </summary>
		public void CloseAll()
		{
			List<UIWindow> tempList = new List<UIWindow>();
			for (int i = 0; i < _stack.Count; i++)
			{
				UIWindow window = _stack[i];
				if (window.DontDestroy == false)
					tempList.Add(window);
			}

			for (int i = 0; i < tempList.Count; i++)
			{
				UIWindow window = tempList[i];
				CloseWindow(window.GetType());
			}
		}

		private void OnWindowPrepare(UIWindow window)
		{
			window.OnRefresh();
			OnSortWindowDepth(window.WindowLayer);
			OnSetWindowVisible(window.WindowLayer);
		}
		private void OnSortWindowDepth(int layer)
		{
			int depth = layer;
			for (int i = 0; i < _stack.Count; i++)
			{
				if (_stack[i].WindowLayer == layer)
				{
					_stack[i].Depth = depth;
					depth += 100; //注意：每次递增100深度
				}
			}
		}
		private void OnSetWindowVisible(int layer)
		{
			bool isHideNext = false;
			for (int i = _stack.Count - 1; i >= 0; i--)
			{
				UIWindow window = _stack[i];
				if (window.WindowLayer == layer)
				{
					if (isHideNext == false)
					{
						window.Visible = true;
						if (window.IsPrepare && window.IsOpen && window.FullScreen)
							isHideNext = true;
					}
					else
					{
						window.Visible = false;
					}
				}
			}
		}

		private UIWindow CreateInstance(Type type)
		{
			UIWindow window = Activator.CreateInstance(type) as UIWindow;
			if (window == null)
				throw new Exception($"Window {type.FullName} create instance failed.");

			WindowAttribute attribute = Attribute.GetCustomAttribute(type, typeof(WindowAttribute)) as WindowAttribute;
			if (attribute == null)
				throw new Exception($"Window {type.FullName} not found {nameof(WindowAttribute)} attribute.");

			window.Init(type.Name, attribute.WindowLayer, attribute.DontDestroy, attribute.FullScreen);
			return window;
		}
		private UIWindow GetWindow(string name)
		{
			for (int i = 0; i < _stack.Count; i++)
			{
				UIWindow window = _stack[i];
				if (window.WindowName == name)
					return window;
			}
			return null;
		}
		private bool IsContains(string name)
		{
			for (int i = 0; i < _stack.Count; i++)
			{
				UIWindow window = _stack[i];
				if (window.WindowName == name)
					return true;
			}
			return false;
		}
		private void Push(UIWindow window)
		{
			// 如果已经存在
			if (IsContains(window.WindowName))
				throw new System.Exception($"Window {window.WindowName} is exist.");

			// 获取插入到所属层级的位置
			int insertIndex = -1;
			for (int i = 0; i < _stack.Count; i++)
			{
				if (window.WindowLayer == _stack[i].WindowLayer)
					insertIndex = i + 1;
			}

			// 如果没有所属层级，找到相邻层级
			if (insertIndex == -1)
			{
				for (int i = 0; i < _stack.Count; i++)
				{
					if (window.WindowLayer > _stack[i].WindowLayer)
						insertIndex = i + 1;
				}
			}

			// 如果是空栈或没有找到插入位置
			if (insertIndex == -1)
			{
				insertIndex = 0;
			}

			// 最后插入到堆栈
			_stack.Insert(insertIndex, window);
		}
		private void Pop(UIWindow window)
		{
			// 从堆栈里移除
			_stack.Remove(window);
		}
	}
}