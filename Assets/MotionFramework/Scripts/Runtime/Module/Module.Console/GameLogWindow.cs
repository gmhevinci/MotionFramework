﻿//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Reference;

namespace MotionFramework.Console
{
	[ConsoleAttribute("游戏日志", 101)]
	internal class GameLogWindow : IConsoleWindow
	{
		private class LogWrapper : IReference
		{
			public LogType Type;
			public string Log;
			public void OnRelease()
			{
				Log = string.Empty;
			}
		}

		/// <summary>
		/// 日志最大显示数量
		/// </summary>
		private const int LOG_MAX_COUNT = 2000;

		/// <summary>
		/// 日志集合
		/// </summary>
		private List<LogWrapper> _logs = new List<LogWrapper>();

		private int _totalCount = 0;
		private int _logCount = 0;
		private int _warningCount = 0;
		private int _errorCount = 0;

		// GUI相关
		private bool _showLog = true;
		private bool _showWarning = true;
		private bool _showError = true;
		private Vector2 _scrollPos = Vector2.zero;

		public GameLogWindow()
		{
			// 注册UnityEngine日志系统
			Application.logMessageReceived += HandleUnityEngineLog;
		}
		void IConsoleWindow.OnGUI()
		{
			GUILayout.BeginHorizontal();
			_showLog = ConsoleGUI.Toggle($"Log ({_logCount})", _showLog);
			_showWarning = ConsoleGUI.Toggle($"Warning ({_warningCount})", _showWarning);
			_showError = ConsoleGUI.Toggle($"Error ({_errorCount})", _showError);
			GUILayout.EndHorizontal();

			float offset = ConsoleGUI.ToolbarStyle.fixedHeight;
			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos, offset);
			for (int i = 0; i < _logs.Count; i++)
			{
				LogWrapper wrapper = _logs[i];
				if (wrapper.Type == LogType.Log)
				{
					if (_showLog)
						ConsoleGUI.Lable(wrapper.Log);
				}
				else if (wrapper.Type == LogType.Warning)
				{
					if (_showWarning)
						ConsoleGUI.YellowLable(wrapper.Log);
				}
				else if (wrapper.Type == LogType.Assert || wrapper.Type == LogType.Error || wrapper.Type == LogType.Exception)
				{
					if (_showError)
						ConsoleGUI.RedLable(wrapper.Log);
				}
			}
			ConsoleGUI.EndScrollView();
		}

		private void HandleUnityEngineLog(string logString, string stackTrace, LogType type)
		{
			LogWrapper wrapper = ReferencePool.Spawn<LogWrapper>();
			wrapper.Type = type;

			_totalCount++;
			if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception)
				wrapper.Log = $"[{_totalCount}] " + logString + "\n" + stackTrace;
			else
				wrapper.Log = $"[{_totalCount}] " + logString;

			if (type == LogType.Log)
				_logCount++;
			else if (type == LogType.Warning)
				_warningCount++;
			else if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception)
				_errorCount++;
			else
				throw new NotImplementedException(type.ToString());

			_logs.Add(wrapper);
			if (_logs.Count > LOG_MAX_COUNT)
			{
				ReferencePool.Release(_logs[0]);
				_logs.RemoveAt(0);
			}
		}
	}
}