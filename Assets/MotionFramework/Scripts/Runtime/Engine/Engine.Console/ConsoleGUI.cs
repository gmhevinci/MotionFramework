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
	public static class ConsoleGUI
	{
		private static bool _initGlobalStyle = false;

		public static GUIStyle ToolbarStyle { private set; get; }
		public static GUIStyle ButtonStyle { private set; get; }
		public static GUIStyle ToogleStyle1 { private set; get; }
		public static GUIStyle ToogleStyle2 { private set; get; }
		public static GUIStyle TextFieldStyle { private set; get; }
		public static GUIStyle LableStyle { private set; get; }
		public static GUIStyle RichLabelStyle { private set; get; }
		public static int RichLabelFontSize { private set; get; }

		/// <summary>
		/// 创建一些高度和字体大小固定的控件样式
		/// </summary>
		internal static void InitGlobalStyle()
		{
			if (_initGlobalStyle == false)
			{
				_initGlobalStyle = true;

				ToolbarStyle = new GUIStyle(GUI.skin.button);
				ToolbarStyle.fontSize = 28;
				ToolbarStyle.fixedHeight = 40;

				ButtonStyle = new GUIStyle(GUI.skin.button);
				ButtonStyle.fontSize = 28;
				ButtonStyle.fixedHeight = 40;

				ToogleStyle1 = new GUIStyle(GUI.skin.button);
				ToogleStyle1.fontSize = 26;
				ToogleStyle1.fixedHeight = 35;

				ToogleStyle2 = new GUIStyle(GUI.skin.box);
				ToogleStyle2.fontSize = 26;
				ToogleStyle2.fixedHeight = 35;

				TextFieldStyle = new GUIStyle(GUI.skin.textField);
				TextFieldStyle.fontSize = 22;
				TextFieldStyle.fixedHeight = 30;

				LableStyle = new GUIStyle(GUI.skin.label);
				LableStyle.fontSize = 24;

				RichLabelStyle = GUIStyle.none;
				RichLabelStyle.richText = true;
				RichLabelFontSize = 24;
			}
		}

		public static Vector2 BeginScrollView(Vector2 pos, int fixedViewHeight)
		{
			float scrollWidth = Screen.width;
			float scrollHeight = Screen.height - ButtonStyle.fixedHeight - fixedViewHeight - 10;
			return GUILayout.BeginScrollView(pos, GUILayout.Width(scrollWidth), GUILayout.Height(scrollHeight));
		}
		public static void EndScrollView()
		{
			GUILayout.EndScrollView();
		}
		public static bool Toggle(string name, bool checkFlag)
		{
			GUIStyle style = checkFlag ? ToogleStyle1 : ToogleStyle2;
			if (GUILayout.Button(name, style))
			{
				checkFlag = !checkFlag;
			}
			return checkFlag;
		}
		public static void Lable(string text)
		{
			GUILayout.Label($"<size={RichLabelFontSize}><color=white>{text}</color></size>", RichLabelStyle);
		}
		public static void RedLable(string text)
		{
			GUILayout.Label($"<size={RichLabelFontSize}><color=red>{text}</color></size>", RichLabelStyle);
		}
		public static void YellowLable(string text)
		{
			GUILayout.Label($"<size={RichLabelFontSize}><color=yellow>{text}</color></size>", RichLabelStyle);
		}
	}
}