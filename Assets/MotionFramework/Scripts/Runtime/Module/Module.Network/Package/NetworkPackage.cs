//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Network
{
	public class NetworkPackage : INetworkPackage
	{
		public bool IsHotfixPackage { set; get; }

		/// <summary>
		/// 消息ID
		/// </summary>
		public int MsgID { set; get; }

		/// <summary>
		/// 消息对象
		/// </summary>
		public System.Object MsgObj { set; get; }

		/// <summary>
		/// 包体数据
		/// </summary>
		public byte[] BodyBytes { set; get; }
	}
}