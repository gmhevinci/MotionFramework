//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Network
{
	public interface INetworkPackage
	{
		/// <summary>
		/// 是否为热更消息
		/// </summary>
		bool IsHotfixPackage { set; get; }
	}
}