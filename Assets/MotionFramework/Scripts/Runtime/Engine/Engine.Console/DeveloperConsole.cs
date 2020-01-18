//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Utility;
using UnityEngine;

namespace MotionFramework.Console
{
	/// <summary>
	/// 控制台
	/// </summary>
	public static class DeveloperConsole
	{
		private class WindowWrapper : IComparer<WindowWrapper>, IComparable<WindowWrapper>
		{
			public Type ClassType;
			public string Title;
			public int Priority;
			public IConsoleWindow Instance;

			public int CompareTo(WindowWrapper other)
			{
				return Compare(this, other);
			}
			public int Compare(WindowWrapper a, WindowWrapper b)
			{
				return a.Priority.CompareTo(b.Priority);
			}
		}

		/// <summary>
		/// 控制台节点列表
		/// </summary>
		private readonly static List<WindowWrapper> _wrappers = new List<WindowWrapper>();
		private static bool _isStart = false;

		// GUI相关
		private static bool _visibleToggle = false;
		private static int _showIndex = 0;
		private static Texture _bgTexture;
		private static string[] _toolbarTitles;

		/// <summary>
		/// 初始化控制台
		/// </summary>
		public static void Initialize()
		{
			// 加载背景纹理
			string textureName = "console_background";
			_bgTexture = Resources.Load<Texture>(textureName);
			if (_bgTexture == null)
				UnityEngine.Debug.LogWarning($"Not found {textureName} texture in Resources folder.");

			// 获取所有调试类
			List<Type> allTypes = AssemblyUtility.GetAssignableAttributeTypes(typeof(IConsoleWindow), typeof(ConsoleAttribute));
			for (int i = 0; i < allTypes.Count; i++)
			{
				ConsoleAttribute attribute = (ConsoleAttribute)Attribute.GetCustomAttribute(allTypes[i], typeof(ConsoleAttribute));
				WindowWrapper wrapper = new WindowWrapper()
				{
					ClassType = allTypes[i],
					Title = attribute.Title,
					Priority = attribute.Order,
				};
				_wrappers.Add(wrapper);
			}

			// 根据优先级排序
			_wrappers.Sort();

			// 创建实例类
			for (int i = 0; i < _wrappers.Count; i++)
			{
				WindowWrapper wrapper = _wrappers[i];
				wrapper.Instance = (IConsoleWindow)Activator.CreateInstance(wrapper.ClassType);
			}

			// 标题列表
			List<string> titles = new List<string>();
			for (int i = 0; i < _wrappers.Count; i++)
			{
				titles.Add(_wrappers[i].Title);
			}
			_toolbarTitles = titles.ToArray();
		}

		/// <summary>
		/// 绘制GUI
		/// </summary>
		public static void DrawGUI()
		{
			// 注意：GUI接口只能在OnGUI内部使用
			ConsoleGUI.InitGlobalStyle();

			if (_isStart == false)
			{
				_isStart = true;
				for (int i = 0; i < _wrappers.Count; i++)
				{
					WindowWrapper wrapper = _wrappers[i];
					wrapper.Instance.OnStart();
				}
			}

			GUILayout.BeginHorizontal();
			{
				// 绘制背景
				if (_visibleToggle && _bgTexture != null)
					GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgTexture, ScaleMode.StretchToFill, true);

				// 显示开关
				if (GUILayout.Button("X", ConsoleGUI.ButtonStyle, GUILayout.Width(ConsoleGUI.ButtonStyle.fixedHeight)))
					_visibleToggle = !_visibleToggle;
				if (_visibleToggle == false)
					return;

				// 绘制按钮栏
				_showIndex = GUILayout.Toolbar(_showIndex, _toolbarTitles, ConsoleGUI.ToolbarStyle);
			}
			GUILayout.EndHorizontal();

			// 绘制选中窗口
			for (int i = 0; i < _wrappers.Count; i++)
			{
				if (_showIndex != i)
					continue;
				WindowWrapper wrapper = _wrappers[i];
				wrapper.Instance.OnGUI();
			}
		}
	}
}