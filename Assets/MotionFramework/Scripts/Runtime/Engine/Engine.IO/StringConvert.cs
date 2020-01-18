//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if MOTION_SERVER
	using System.Numerics;
#else
	using UnityEngine;
#endif

namespace MotionFramework.IO
{
	public static class StringConvert
	{
		/// <summary>
		/// 正则表达式
		/// </summary>
		private static Regex REGEX = new Regex(@"\{[-+]?[0-9]+\.?[0-9]*\}", RegexOptions.IgnoreCase);

		/// <summary>
		/// 替换掉字符串里的特殊字符
		/// </summary>
		public static string ReplaceSpecialChar(string str)
		{
			return str.Replace("\\n", "\n").Replace("\\r", "'\r").Replace("\\t", "\t");
		}

		/// <summary>
		/// 字符串转换为字符串
		/// </summary>
		public static string StringToString(string str)
		{
			return str;
		}

		/// <summary>
		/// 字符串转换为BOOL
		/// </summary>
		public static bool StringToBool(string str)
		{
			int value = (int)Convert.ChangeType(str, typeof(int));
			return value > 0;
		}

		/// <summary>
		/// 字符串转换为数值
		/// </summary>
		public static T StringToValue<T>(string str)
		{
			return (T)Convert.ChangeType(str, typeof(T));
		}

		/// <summary>
		/// 字符串转换为数值列表
		/// </summary>
		/// <param name="separator">分隔符</param>
		public static List<T> StringToValueList<T>(string str, char separator)
		{
			List<T> result = new List<T>();
			if (!String.IsNullOrEmpty(str))
			{
				string[] splits = str.Split(separator);
				foreach (string split in splits)
				{
					if (!String.IsNullOrEmpty(split))
					{
						result.Add((T)Convert.ChangeType(split, typeof(T)));
					}
				}
			}
			return result;
		}

		/// <summary>
		/// 字符串转为字符串列表
		/// </summary>
		public static List<string> StringToStringList(string str, char separator)
		{
			List<string> result = new List<string>();
			if (!String.IsNullOrEmpty(str))
			{
				string[] splits = str.Split(separator);
				foreach (string split in splits)
				{
					if (!String.IsNullOrEmpty(split))
					{
						result.Add(split);
					}
				}
			}
			return result;
		}

		/// <summary>
		/// 转换为枚举
		/// 枚举索引转换为枚举类型
		/// </summary>
		public static T IndexToEnum<T>(string index) where T : IConvertible
		{
			int enumIndex = (int)Convert.ChangeType(index, typeof(int));
			return IndexToEnum<T>(enumIndex);
		}

		/// <summary>
		/// 转换为枚举
		/// 枚举索引转换为枚举类型
		/// </summary>
		public static T IndexToEnum<T>(int index) where T : IConvertible
		{
			if (Enum.IsDefined(typeof(T), index) == false)
			{
				throw new ArgumentException($"Enum {typeof(T)} is not defined index {index}");
			}
			return (T)Enum.ToObject(typeof(T), index);
		}

		/// <summary>
		/// 转换为枚举
		/// 枚举名称转换为枚举类型
		/// </summary>
		public static T NameToEnum<T>(string name)
		{
			if (Enum.IsDefined(typeof(T), name) == false)
			{
				throw new ArgumentException($"Enum {typeof(T)} is not defined name {name}");
			}
			return (T)Enum.Parse(typeof(T), name);
		}

		/// <summary>
		/// 字符串转换为参数列表
		/// </summary>
		public static List<float> StringToParams(string str)
		{
			List<float> result = new List<float>();
			MatchCollection matches = REGEX.Matches(str);
			for (int i = 0; i < matches.Count; i++)
			{
				string value = matches[i].Value.Trim('{', '}');
				result.Add(StringToValue<float>(value));
			}
			return result;
		}

		/// <summary>
		/// 字符串转换为向量
		/// </summary>
		public static Vector3 StringToVector3(string str, char separator)
		{
			List<float> values = StringToValueList<float>(str, separator);
			return new Vector3(values[0], values[1], values[2]);
		}
	}
}
