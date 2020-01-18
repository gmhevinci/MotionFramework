//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Text;

namespace MotionFramework.IO
{
	/// </summary>
	/// 字符串操作类
	/// 注意：Span<T>需要C# 7.2支持
	/// </summary>
	public static class StringOperate
	{
		private static string _operateString;
		private static int _operateIndex = 0;

		/// <summary>
		/// 设置要处理的字符串
		/// </summary>
		public static void SetOperateString(string content)
		{
			_operateString = content;
			_operateIndex = 0;
		}

		public static bool NextFloat(char separator, out float value)
		{
			value = 0;

#if MOTION_SERVER
			ReadOnlySpan<char> span = MoveNext(separator);		
#else
			string span = MoveNext(separator);
#endif

			if (span == null)
			{
				return false;
			}
			else
			{
				value = Single.Parse(span);
				return true;
			}
		}
		public static bool NextDouble(char separator, out double value)
		{
			value = 0;

#if MOTION_SERVER
			ReadOnlySpan<char> span = MoveNext(separator);		
#else
			string span = MoveNext(separator);
#endif

			if (span == null)
			{
				return false;
			}
			else
			{
				value = Double.Parse(span);
				return true;
			}
		}
		public static bool NextInt(char separator, out int value)
		{
			value = 0;

#if MOTION_SERVER
			ReadOnlySpan<char> span = MoveNext(separator);
#else
			string span = MoveNext(separator);
#endif

			if (span == null)
			{
				return false;
			}
			else
			{
				value = Int32.Parse(span);
				return true;
			}
		}
		public static bool NextLong(char separator, out long value)
		{
			value = 0;

#if MOTION_SERVER
			ReadOnlySpan<char> span = MoveNext(separator);
#else
			string span = MoveNext(separator);		
#endif

			if (span == null)
			{
				return false;
			}
			else
			{
				value = Int64.Parse(span);
				return true;
			}
		}
		public static bool NextString(char separator, out string value)
		{
			value = null;

#if MOTION_SERVER
			ReadOnlySpan<char> span = MoveNext(separator);
#else
			string span = MoveNext(separator);
#endif

			if (span == null)
			{
				return false;
			}
			else
			{
				value = span.ToString();
				return true;
			}
		}

#if MOTION_SERVER
		private static ReadOnlySpan<char> MoveNext(char separator)
#else
		private static string MoveNext(char separator)
#endif
		{
			int beginIndex = _operateIndex;

			for (int i = _operateIndex; i < _operateString.Length; i++)
			{
				bool isLastChar = _operateIndex == _operateString.Length - 1;
				bool isSeparatorChar = _operateString[i] == separator;

				if (isSeparatorChar || isLastChar)
				{
					if (isLastChar && isSeparatorChar == false)
						_operateIndex++;

					int charCount = _operateIndex - beginIndex;
					if (charCount == 0)
					{
						throw new InvalidOperationException($"Invalid operate string : {_operateString}");
					}

					_operateIndex++;

#if MOTION_SERVER
					return _operateString.AsSpan(beginIndex, charCount);
#else
					return _operateString.Substring(beginIndex, charCount);				
#endif
				}
				else
				{
					_operateIndex++;
				}
			}

			return null; //移动失败返回NULL
		}
	}
}
