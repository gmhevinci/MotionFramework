//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework
{
	internal static class MotionLog
	{
		private static System.Action<ELogLevel, string> _callback;

		/// <summary>
		/// 监听日志
		/// </summary>
		public static void RegisterCallback(System.Action<ELogLevel, string> callback)
		{
			_callback += callback;
		}

		/// <summary>
		/// 日志
		/// </summary>
		public static void Log(string info)
		{
			_callback?.Invoke(ELogLevel.Log, $"[MotionLog] {info}");
		}

		/// <summary>
		/// 警告
		/// </summary>
		public static void Warning(string info)
		{
			_callback?.Invoke(ELogLevel.Warning, $"[MotionLog] {info}");
		}

		/// <summary>
		/// 错误
		/// </summary>
		public static void Error(string info)
		{
			_callback?.Invoke(ELogLevel.Error, $"[MotionLog] {info}");
		}

		/// <summary>
		/// 异常
		/// </summary>
		public static void Exception(string info)
		{
			_callback?.Invoke(ELogLevel.Exception, $"[MotionLog] {info}");
		}
	}
}