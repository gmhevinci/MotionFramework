//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;

namespace MotionFramework.Window
{
	[AttributeUsage(AttributeTargets.Class)]
	public class WindowAttribute : Attribute
	{
		/// <summary>
		/// 窗口层级
		/// </summary>
		public int WindowLayer;

		public WindowAttribute(int windowLayer)
		{
			WindowLayer = windowLayer;
		}
	}
}