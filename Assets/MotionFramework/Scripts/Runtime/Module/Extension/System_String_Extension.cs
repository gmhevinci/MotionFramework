//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace System
{
	public static partial class System_String_Extension
	{
		/// <summary>
		/// 移除首个字符
		/// </summary>
		public static string RemoveFirstChar(this System.String str)
		{
			if (string.IsNullOrEmpty(str))
				return str;
			return str.Substring(1);
		}

		/// <summary>
		/// 移除末尾字符
		/// </summary>
		public static string RemoveLastChar(this System.String str)
		{
			if (string.IsNullOrEmpty(str))
				return str;
			return str.Substring(0, str.Length - 1);
		}

		/// <summary>
		/// 移除后缀名
		/// </summary>
		public static string RemoveExtension(this System.String str)
		{
			if (string.IsNullOrEmpty(str))
				return str;

			int index = str.LastIndexOf(".");
			if (index == -1)
				return str;
			else
				return str.Remove(index); //"assets/config/test.unity3d" --> "assets/config/test"
		}
	}
}