//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace System
{
	public static class System_String_Extention
	{
		/// <summary>
		/// 移除首个字符
		/// </summary>
		public static string RemoveFirstChar(this System.String o)
		{
			return o.Substring(1);
		}

		/// <summary>
		/// 移除末尾字符
		/// </summary>
		public static string RemoveLastChar(this System.String o)
		{
			return o.Substring(0, o.Length - 1);
		}
	}
}