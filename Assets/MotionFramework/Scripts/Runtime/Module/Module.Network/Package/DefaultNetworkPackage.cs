//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Network
{
	public class DefaultNetworkPackage : INetworkPackage
	{
		/// <summary>
		/// 消息ID
		/// </summary>
		public int MsgID { set; get; }

		/// <summary>
		/// 包体数据
		/// </summary>
		public byte[] BodyBytes { set; get; }
	}
}