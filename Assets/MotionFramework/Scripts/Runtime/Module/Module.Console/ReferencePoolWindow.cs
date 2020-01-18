//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Reference;

namespace MotionFramework.Console
{
	[ConsoleAttribute("引用池", 104)]
	internal class ReferencePoolWindow : IConsoleWindow
	{
		// GUI相关
		private Vector2 _scrollPos = Vector2.zero;

		void IConsoleWindow.OnStart()
		{
		}
		void IConsoleWindow.OnGUI()
		{
			var pools = ReferencePool.GetAllCollectors;
			ConsoleGUI.Lable($"池总数：{pools.Count}");

			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos, 30);
			foreach (var pair in pools)
			{
				ConsoleGUI.Lable($"[{pair.Value.ClassType.FullName}] CacheCount = {pair.Value.Count} SpwanCount = {pair.Value.SpawnCount}");
			}
			ConsoleGUI.EndScrollView();
		}
	}
}