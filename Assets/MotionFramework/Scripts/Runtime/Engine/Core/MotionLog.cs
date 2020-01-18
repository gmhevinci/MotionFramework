//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework
{
	public static class MotionLog
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
		/// 输出日志
		/// </summary>
		public static void Log(ELogLevel logLevel, string log, params object[] args)
		{
			if (_callback != null)
			{
				string content = string.Format(log, args);
				_callback.Invoke(logLevel, content);
			}
		}

		/// <summary>
		/// 输出日志
		/// </summary>
		public static void Log(ELogLevel logLevel, string log)
		{
			if (_callback != null)
			{
				_callback.Invoke(logLevel, log);
			}
		}
	}
}