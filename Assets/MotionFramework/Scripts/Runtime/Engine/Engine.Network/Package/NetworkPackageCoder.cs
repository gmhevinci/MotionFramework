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
		protected ByteBuffer _sendBuffer;
		protected ByteBuffer _receiveBuffer;
		public TcpChannel Channel { private set; get; }
		public int PackageMaxSize { private set; get; }

		/// <summary>
		/// 初始化解码器
		/// </summary>
		public void InitCoder(TcpChannel channel, int packageMaxSize)
		{
			Channel = channel;
			PackageMaxSize = packageMaxSize;

			// 注意：字节缓冲区长度，推荐4倍最大包体长度
			int byteBufferSize = packageMaxSize * 4;
			_sendBuffer = new ByteBuffer(byteBufferSize);
			_receiveBuffer = new ByteBuffer(byteBufferSize);
		}

		/// <summary>
		/// 释放
		/// </summary>
		public virtual void Dispose()
		{
			if (_sendBuffer != null)
				_sendBuffer.Clear();

			if (_receiveBuffer != null)
				_receiveBuffer.Clear();
		}

		/// <summary>
		/// 编码
		/// </summary>
		/// <param name="packageObj">需要编码的包裹对象</param>
		public abstract void Encode(System.Object packageObj);

		/// <summary>
		/// 解码
		/// </summary>
		/// <param name="packageObjList">解码成功后的包裹对象列表</param>
		public abstract void Decode(List<System.Object> packageObjList);


		#region 字节缓冲区处理接口
		public void SetReceiveDataSize(int size)
		{
			_receiveBuffer.WriterIndex += size;
		}

		public void ClearReceiveBuffer()
		{
			_receiveBuffer.Clear();
		}
		public byte[] GetReceiveBuffer()
		{
			return _receiveBuffer.Buf;
		}
		public int GetReceiveBufferCapacity()
		{
			return _receiveBuffer.Capacity;
		}
		public int GetReceiveBufferWriterIndex()
		{
			return _receiveBuffer.WriterIndex;
		}
		public int GetReceiveBufferWriteableBytes()
		{
			return _receiveBuffer.WriteableBytes();
		}
		public int GetReceiveBufferReadableBytes()
		{
			return _receiveBuffer.ReadableBytes();
		}

		public void ClearSendBuffer()
		{
			_sendBuffer.Clear();
		}
		public byte[] GetSendBuffer()
		{
			return _sendBuffer.Buf;
		}
		public int GetSendBufferCapacity()
		{
			return _sendBuffer.Capacity;
		}
		public int GetSendBufferWriterIndex()
		{
			return _sendBuffer.WriterIndex;
		}
		public int GetSendBufferWriteableBytes()
		{
			return _sendBuffer.WriteableBytes();
		}
		public int GetSendBufferReadableBytes()
		{
			return _sendBuffer.ReadableBytes();
		}
		#endregion
	}
}