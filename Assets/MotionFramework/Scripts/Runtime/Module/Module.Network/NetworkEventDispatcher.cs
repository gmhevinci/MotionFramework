//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Event;

namespace MotionFramework.Network
{
	internal static class NetworkEventDispatcher
	{
		public static void SendBeginConnectMsg()
		{
			NetworkEventMessageDefine.BeginConnect msg = new NetworkEventMessageDefine.BeginConnect();
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendConnectSuccessMsg()
		{
			NetworkEventMessageDefine.ConnectSuccess msg = new NetworkEventMessageDefine.ConnectSuccess();
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendConnectFailMsg(string error)
		{
			NetworkEventMessageDefine.ConnectFail msg = new NetworkEventMessageDefine.ConnectFail();
			msg.Error = error;
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendDisconnectMsg()
		{
			NetworkEventMessageDefine.Disconnect msg = new NetworkEventMessageDefine.Disconnect();
			EventManager.Instance.SendMessage(msg);
		}
	}
}