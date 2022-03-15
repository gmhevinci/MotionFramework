//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Pool;
using MotionFramework.Resource;
using YooAsset;

namespace MotionFramework.Console
{
	[ConsoleAttribute("游戏对象池", 105)]
	internal class GameObjectPoolWindow : IConsoleWindow
	{
		// GUI相关
		private Vector2 _scrollPos = Vector2.zero;

		void IConsoleWindow.OnGUI()
		{
			// 如果游戏模块没有创建
			if (MotionEngine.Contains(typeof(GameObjectPoolManager)) == false)
			{
				ConsoleGUI.YellowLable($"{nameof(GameObjectPoolManager)} is not create.");
				return;
			}

			var pools = GameObjectPoolManager.Instance.GetAllCollectors;
			ConsoleGUI.Lable($"池总数：{pools.Count}");

			float offset = ConsoleGUI.LableStyle.fontSize;
			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos, offset);
			foreach (var pair in pools)
			{
				string content = $"[{pair.Value.Location}] CacheCount = {pair.Value.CacheCount} SpwanCount = {pair.Value.SpawnCount}";
				if (pair.Value.States == EOperationStatus.Failed)
					ConsoleGUI.RedLable(content);
				else
					ConsoleGUI.Lable(content);
			}
			ConsoleGUI.EndScrollView();
		}
	}
}