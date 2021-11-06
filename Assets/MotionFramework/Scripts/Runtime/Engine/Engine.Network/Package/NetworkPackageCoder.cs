//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.IO;

namespace MotionFramework.Network
{
	/// <summary>
	/// 网络包编码解码器
	/// </summary>
	public abstract class NetworkPackageCoder
	{
		private TcpChannel _channel;

		/// <summary>
		/// 包体的最大尺寸
		/// </summary>
		public int PackageBodyMaxSize { private set; get; }

		/// <summary>
		/// 初始化编码解码器
		/// </summary>
		public void InitCoder(TcpChannel channel, int packageBodyMaxSize)
		{
			_channel = channel;
			PackageBodyMaxSize = packageBodyMaxSize;
		}

		/// <summary>
		/// 获取包头的尺寸
		/// </summary>
		public abstract int GetPackageHeaderSize();

		/// <summary>
		/// 编码
		/// </summary>
		/// <param name="sendBuffer">编码填充的字节缓冲区</param>
		/// <param name="sendPackage">发送的包裹</param>
		public abstract void Encode(ByteBuffer sendBuffer, INetworkPackage sendPackage);

		/// <summary>
		/// 解码
		/// </summary>
		/// <param name="receiveBuffer">解码需要的字节缓冲区</param>
		/// <param name="receivePackages">接收的包裹列表</param>
		public abstract void Decode(ByteBuffer receiveBuffer, List<INetworkPackage> receivePackages);

		/// <summary>
		/// 捕捉错误异常
		/// </summary>
		protected void HandleError(bool isDispose, string error)
		{
			_channel.HandleError(isDispose, error);
		}
	}
}