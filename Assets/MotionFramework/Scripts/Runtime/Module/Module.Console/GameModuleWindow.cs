//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Console
{
	[ConsoleAttribute("游戏模块", 100)]
	internal class GameModuleWindow : IConsoleWindow
	{
		// GUI相关
		private Vector2 _scrollPos = Vector2.zero;
		
		void IConsoleWindow.OnStart()
		{
		}
		void IConsoleWindow.OnGUI()
		{
			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos, 0);
			MotionEngine.DrawGUI();
			ConsoleGUI.EndScrollView();
		}
	}
}