//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Text;
using System.Diagnostics;
namespace MotionFramework.Utility
{
	public static class ProfilerUtility
	{
		private static string _watchName;
		private static long _refMilliseconds;
		private static Stopwatch _watch;

		/// <summary>
		/// 开启性能测试
		/// </summary>
		/// <param name="name">测试名称</param>
		/// <param name="refMilliseconds">推荐的最小毫秒数</param>
		[Conditional("DEBUG")]
		public static void BeginWatch(string name, long refMilliseconds = long.MaxValue)
		{
			if (_watch != null)
			{
				UnityEngine.Debug.LogError($"Last watch is not over : {_watchName}");
			}

			_watchName = name;
			_refMilliseconds = refMilliseconds;
			_watch = new Stopwatch();
			_watch.Start();
		}

		/// <summary>
		/// 结束性能测试
		/// 说明：当耗费的毫秒数超过推荐值则输出为警告
		/// </summary>
		[Conditional("DEBUG")]
		public static void EndWatch()
		{
			if (_watch != null)
			{
				_watch.Stop();
				string logInfo = $"{_watchName} took {_watch.ElapsedMilliseconds} ms";
				if (_watch.ElapsedMilliseconds > _refMilliseconds)
					UnityEngine.Debug.Log(logInfo);
				else
					UnityEngine.Debug.LogWarning(logInfo);
				_watch = null;
			}
		}
	}
}