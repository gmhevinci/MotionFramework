//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;

namespace MotionFramework.Network
{
	[AttributeUsage(AttributeTargets.Class)]
	public class NetworkMessageAttribute : Attribute
	{
		public int MsgID;

		public NetworkMessageAttribute(int msgID)
		{
			MsgID = msgID;
		}
	}
}