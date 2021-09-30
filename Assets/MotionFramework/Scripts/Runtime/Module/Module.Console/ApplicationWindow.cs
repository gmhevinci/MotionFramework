//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace MotionFramework.Console
{
	[ConsoleAttribute("应用详情", 102)]
	internal class ApplicationWindow : IConsoleWindow
	{
		// GUI相关
		private Vector2 _scrollPos = Vector2.zero;
		private int _timeScaleLevel = 5;

		void IConsoleWindow.OnGUI()
		{
			int space = 15;

			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos);

			// 时间缩放相关
			GUILayout.Space(space);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Reset", ConsoleGUI.ButtonStyle, GUILayout.Width(100)))
			{
				_timeScaleLevel = 5;
				SetTimeScale(_timeScaleLevel);
			}
			if (GUILayout.Button("+", ConsoleGUI.ButtonStyle, GUILayout.Width(100)))
			{
				_timeScaleLevel++;
				_timeScaleLevel = Mathf.Clamp(_timeScaleLevel, 0, 9);
				SetTimeScale(_timeScaleLevel);
			}
			if (GUILayout.Button("-", ConsoleGUI.ButtonStyle, GUILayout.Width(100)))
			{
				_timeScaleLevel--;
				_timeScaleLevel = Mathf.Clamp(_timeScaleLevel, 0, 9);
				SetTimeScale(_timeScaleLevel);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Elapse Time : {GetElapseTime()}");
			ConsoleGUI.Lable($"Time Scale : {Time.timeScale}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Unity Version : {Application.unityVersion}");
			ConsoleGUI.Lable($"Unity Pro License : {Application.HasProLicense()}");
			ConsoleGUI.Lable($"Application Version : {Application.version}");
			ConsoleGUI.Lable($"Application Install Path : {Application.dataPath}");
			ConsoleGUI.Lable($"Application Persistent Path : {Application.persistentDataPath}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"OS : {SystemInfo.operatingSystem}");
			ConsoleGUI.Lable($"OS Memory : {SystemInfo.systemMemorySize / 1024f:f1}GB");
			ConsoleGUI.Lable($"CPU : {SystemInfo.processorType}");
			ConsoleGUI.Lable($"CPU Core : {SystemInfo.processorCount}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Device UID : {SystemInfo.deviceUniqueIdentifier}");
			ConsoleGUI.Lable($"Device Model : {SystemInfo.deviceModel}");
			ConsoleGUI.Lable($"Device Name : {SystemInfo.deviceName}");
			ConsoleGUI.Lable($"Device Type : {SystemInfo.deviceType}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Graphics Device Name : {SystemInfo.graphicsDeviceName}");
			ConsoleGUI.Lable($"Graphics Device Type : {SystemInfo.graphicsDeviceType}");
			ConsoleGUI.Lable($"Graphics Memory : {SystemInfo.graphicsMemorySize / 1024f:f1}GB");
			ConsoleGUI.Lable($"Graphics Shader Level : {SystemInfo.graphicsShaderLevel}");
			ConsoleGUI.Lable($"Multi-threaded Rendering : {SystemInfo.graphicsMultiThreaded}");
			ConsoleGUI.Lable($"Max Cubemap Size : {SystemInfo.maxCubemapSize}");
			ConsoleGUI.Lable($"Max Texture Size : {SystemInfo.maxTextureSize}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Supports Accelerometer : {SystemInfo.supportsAccelerometer}"); //加速计硬件
			ConsoleGUI.Lable($"Supports Gyroscope : {SystemInfo.supportsGyroscope}"); //陀螺仪硬件
			ConsoleGUI.Lable($"Supports Audio : {SystemInfo.supportsAudio}"); //音频硬件
			ConsoleGUI.Lable($"Supports GPS : {SystemInfo.supportsLocationService}"); //GPS硬件

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Screen DPI : {Screen.dpi}");
			ConsoleGUI.Lable($"Game Resolution : {Screen.width} x {Screen.height}");
			ConsoleGUI.Lable($"Device Resolution : {Screen.currentResolution.width} x {Screen.currentResolution.height}");
			ConsoleGUI.Lable($"Graphics Quality : {QualitySettings.names[QualitySettings.GetQualityLevel()]}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Total Memory : {GetTotalMemory() / 1048576}MB");			
			ConsoleGUI.Lable($" - Unity Memory : {Profiler.GetTotalReservedMemoryLong() / 1048576}MB");
			ConsoleGUI.Lable($" - Mono Memory : {Profiler.GetMonoHeapSizeLong() / 1048576}MB");
			ConsoleGUI.Lable($" - GfxDriver Memory : {Profiler.GetAllocatedMemoryForGraphicsDriver() / 1048576}MB");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Battery Level : {SystemInfo.batteryLevel}");
			ConsoleGUI.Lable($"Battery Status : {SystemInfo.batteryStatus}");
			ConsoleGUI.Lable($"Network Status : {GetNetworkState()}");

			ConsoleGUI.EndScrollView();
		}

		private long GetTotalMemory()
		{
			// Total: Unity+Mono+GfxDriver（未包含Audio+Video）
			return Profiler.GetTotalReservedMemoryLong() + Profiler.GetMonoHeapSizeLong() + Profiler.GetAllocatedMemoryForGraphicsDriver();
		}
		private string GetNetworkState()
		{
			string internetState = string.Empty;
			if (Application.internetReachability == NetworkReachability.NotReachable)
				internetState = "not reachable";
			else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
				internetState = "carrier data network";
			else if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
				internetState = "local area network";
			return internetState;
		}
		private string GetElapseTime()
		{
			int day = (int)(Time.realtimeSinceStartup / 86400f);
			int hour = (int)((Time.realtimeSinceStartup % 86400f) / 3600f);
			int sec = (int)(((Time.realtimeSinceStartup % 86400f) % 3600f) / 60f);
			return $"{day}天{hour}小时{sec}分";
		}
		private void SetTimeScale(int timeScaleLevel)
		{
			if (timeScaleLevel == 5)
			{
				Time.timeScale = 1f;
			}
			else if (timeScaleLevel > 5)
			{
				Time.timeScale = timeScaleLevel - 4;
			}
			else
			{
				Time.timeScale = timeScaleLevel / 5f;
			}
		}
	}
}