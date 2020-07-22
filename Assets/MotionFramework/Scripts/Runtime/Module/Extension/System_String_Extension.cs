//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
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
	}
}