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

		// GUI相关
		private static bool _visibleToggle = false;
		private static int _showIndex = 0;
		private static Texture _bgTexture;
		private static string[] _toolbarTitles;


		/// <summary>
		/// 初始化控制台
		/// </summary>
		/// <param name="assemblyName">扩展的控制台窗口所在的图集</param>
		public static void Initialize(string assemblyName = AssemblyUtility.UnityDefaultAssemblyName)
		{
			// 加载背景纹理
			string textureName = "console_background";
			_bgTexture = Resources.Load<Texture>(textureName);
			if (_bgTexture == null)
				UnityEngine.Debug.LogWarning($"Not found {textureName} texture in Resources folder.");

			// 获取所有调试类
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
			// 注意：GUI接口只能在OnGUI内部使用
			ConsoleGUI.InitGlobalStyle();

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