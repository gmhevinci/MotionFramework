//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Utility;

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

		// FPS相关
		private static FPSCounter _fpsCounter = null;
		private static int _lastFrame = 0;

		// GUI相关
		private static bool _visible = false;
		private static int _showIndex = 0;
		private static Texture _bgTexture;
		private static string[] _toolbarTitles;

		/// <summary>
		/// 初始化控制台
		/// </summary>
		/// <param name="assemblyName">扩展的控制台窗口所在的程序集</param>
		public static void Initialize(bool showFPS = true, string assemblyName = AssemblyUtility.UnityDefaultAssemblyName)
		{
			if (showFPS)
			{
				_fpsCounter = new FPSCounter();
			}

			// 加载背景纹理
			string textureName = "console_background";
			_bgTexture = Resources.Load<Texture>(textureName);
			if (_bgTexture == null)
				UnityEngine.Debug.LogWarning($"Not found {textureName} texture in Resources folder.");

			// 获取所有控制台窗口类
			List<Type> types = AssemblyUtility.GetAssignableAttributeTypes(AssemblyUtility.MotionFrameworkAssemblyName, typeof(IConsoleWindow), typeof(ConsoleAttribute));
			List<Type> temps = AssemblyUtility.GetAssignableAttributeTypes(assemblyName, typeof(IConsoleWindow), typeof(ConsoleAttribute));
			types.AddRange(temps);
			for (int i = 0; i < types.Count; i++)
			{
				ConsoleAttribute attribute = (ConsoleAttribute)Attribute.GetCustomAttribute(types[i], typeof(ConsoleAttribute));
				WindowWrapper wrapper = new WindowWrapper()
				{
					ClassType = types[i],
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
		/// 绘制控制台
		/// 注意：该接口必须在OnGUI函数内调用
		/// </summary>
		public static void Draw()
		{
			if (_fpsCounter != null)
			{
				if (_lastFrame != Time.frameCount)
				{
					_lastFrame = Time.frameCount;
					_fpsCounter.Update();
				}
			}

			// 注意：GUI接口只能在OnGUI内部使用
			ConsoleGUI.InitGlobalStyle();

			float posX = Screen.safeArea.x;
			float posY = Screen.height - Screen.safeArea.height - Screen.safeArea.y;

			if (_visible == false)
			{
				float wdith = ConsoleGUI.XStyle.fixedWidth;
				float height = ConsoleGUI.XStyle.fixedHeight;

				// 显示按钮
				if (GUI.Button(new Rect(posX + 10, posY + 10, wdith, height), "X", ConsoleGUI.XStyle))
					_visible = true;

				// FPS
				if (_fpsCounter != null)
				{
					int fps = _fpsCounter.GetFPS();
					string text = $"<size={ConsoleGUI.RichLabelFontSize * 2}><color=red>{fps}</color></size>";
					GUI.Label(new Rect(posX + wdith * 1.5f, posY + 5, wdith * 2, height * 2), text, ConsoleGUI.RichLabelStyle);
				}
			}
			else
			{
				Rect windowRect = new Rect(posX, posY, Screen.safeArea.width, Screen.safeArea.height);
				GUI.Window(0, windowRect, DrawWindow, string.Empty);
			}
		}
		private static void DrawWindow(int windowID)
		{
			// 绘制背景
			if (_visible && _bgTexture != null)
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgTexture, ScaleMode.StretchToFill, true);

			GUILayout.BeginHorizontal();
			{
				// 隐藏按钮
				if (GUILayout.Button("X", ConsoleGUI.ButtonStyle, GUILayout.Width(ConsoleGUI.ButtonStyle.fixedHeight)))
					_visible = false;

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