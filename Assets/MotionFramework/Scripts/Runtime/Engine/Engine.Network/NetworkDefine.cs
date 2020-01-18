//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Network
{
	internal class NetworkDefine
	{
		/// <summary>
		/// 网络包体的最大长度
		/// </summary>
		public const int PackageBodyMaxSize = ushort.MaxValue;

		/// <summary>
		/// 字节缓冲区长度（注意：推荐4倍最大包体长度）
		/// </summary>
		public const int ByteBufferSize = PackageBodyMaxSize * 4;

		/// <summary>
		/// 网络请求的超时时间（单位：秒）
		/// </summary>
		public const int WebRequestTimeout = 30;
	}
}