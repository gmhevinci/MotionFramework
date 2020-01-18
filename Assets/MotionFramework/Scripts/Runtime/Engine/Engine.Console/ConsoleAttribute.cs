//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;

namespace MotionFramework.Console
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ConsoleAttribute : Attribute
	{
		/// <summary>
		/// 标题名称
		/// </summary>
		public string Title;

		/// <summary>
		/// 显示顺序
		/// </summary>
		public int Order;

		public ConsoleAttribute(string title, int order)
		{
			Title = title;
			Order = order;
		}
	}
}