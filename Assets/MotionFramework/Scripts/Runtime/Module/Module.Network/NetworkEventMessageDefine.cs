//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Event;

namespace MotionFramework.Network
{
	public class NetworkEventMessageDefine
	{
		/// <summary>
		/// 开始连接
		/// </summary>
		public class BeginConnect : IEventMessage
		{
		}

		/// <summary>
		/// 连接成功
		/// </summary>
		public class ConnectSuccess : IEventMessage
		{
		}

		/// <summary>
		/// 连接失败
		/// </summary>
		public class ConnectFail : IEventMessage
		{
			public string Error;
		}

		/// <summary>
		/// 断开连接
		/// </summary>
		public class Disconnect : IEventMessage
		{
		}
	}
}