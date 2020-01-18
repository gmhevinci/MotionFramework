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

		void IConsoleWindow.OnStart()
		{
		}
		void IConsoleWindow.OnGUI()
		{
			int space = 15;

			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos, 0);

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Unity Version : {Application.unityVersion}");
			ConsoleGUI.Lable($"Unity Pro License : {Application.HasProLicense()}");
			ConsoleGUI.Lable($"Application Version : {Application.version}");
			ConsoleGUI.Lable($"Application Install Path : {Application.dataPath}");
			ConsoleGUI.Lable($"Application Persistent Path : {Application.persistentDataPath}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"OS : {SystemInfo.operatingSystem}");
			ConsoleGUI.Lable($"OS Memory : {SystemInfo.systemMemorySize / 1000}GB");
			ConsoleGUI.Lable($"CPU : {SystemInfo.processorType}");
			ConsoleGUI.Lable($"CPU Core : {SystemInfo.processorCount}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Device Model : {SystemInfo.deviceModel}");
			ConsoleGUI.Lable($"Device Name : {SystemInfo.deviceName}");
			ConsoleGUI.Lable($"Device Type : {SystemInfo.deviceType}");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Graphics Device Name : {SystemInfo.graphicsDeviceName}");
			ConsoleGUI.Lable($"Graphics Device Type : {SystemInfo.graphicsDeviceType}");
			ConsoleGUI.Lable($"Graphics Memory : {SystemInfo.graphicsMemorySize / 1000}GB");
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
			long memory = Profiler.GetTotalReservedMemoryLong() / 1000000;
			ConsoleGUI.Lable($"Total Memory : {memory}MB");
			memory = Profiler.GetTotalAllocatedMemoryLong() / 1000000;
			ConsoleGUI.Lable($"Used Memory : {memory}MB");
			memory = Profiler.GetTotalUnusedReservedMemoryLong() / 1000000;
			ConsoleGUI.Lable($"Free Memory : {memory}MB");
			memory = Profiler.GetMonoHeapSizeLong() / 1000000;
			ConsoleGUI.Lable($"Total Mono Memory : {memory}MB");
			memory = Profiler.GetMonoUsedSizeLong() / 1000000;
			ConsoleGUI.Lable($"Used Mono Memory : {memory}MB");

			GUILayout.Space(space);
			ConsoleGUI.Lable($"Battery Level : {SystemInfo.batteryLevel}");
			ConsoleGUI.Lable($"Battery Status : {SystemInfo.batteryStatus}");
			ConsoleGUI.Lable($"Network Status : {GetNetworkState()}");
			ConsoleGUI.Lable($"Elapse Time : {GetElapseTime()}");
			ConsoleGUI.Lable($"Time Scale : {Time.timeScale}");

			ConsoleGUI.EndScrollView();
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
	}
}