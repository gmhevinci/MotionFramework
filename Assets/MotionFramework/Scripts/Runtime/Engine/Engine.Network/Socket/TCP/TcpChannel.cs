//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MotionFramework.IO;

namespace MotionFramework.Network
{
	public class TcpChannel : IDisposable
	{
		private readonly SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

		private readonly Queue<System.Object> _sendQueue = new Queue<System.Object>(10000);
		private readonly Queue<System.Object> _receiveQueue = new Queue<System.Object>(10000);
		private readonly List<System.Object> _decodeTempList = new List<object>(100);

		private int _packageMaxSize;
		private byte[] _tempBuffer;
		private ByteBuffer _sendBuffer;
		private ByteBuffer _receiveBuffer;
		private NetworkPackageCoder _packageCoder;

		/// <summary>
		/// 通信Socket
		/// </summary>
		private Socket _socket;


		/// <summary>
		/// 初始化频道
		/// </summary>
		public void InitChannel(Socket socket, Type packageCoderType, int packageBodyMaxSize)
		{
			if (packageCoderType == null)
				throw new System.ArgumentException($"packageCoderType is null.");
			if (packageBodyMaxSize <= 0)
				throw new System.ArgumentException($"packageMaxSize is invalid : {packageBodyMaxSize}");

			_socket = socket;
			_socket.NoDelay = true;

			// 创建编码解码器
			_packageCoder = (NetworkPackageCoder)Activator.CreateInstance(packageCoderType);
			_packageCoder.InitCoder(this, packageBodyMaxSize);
			_packageMaxSize = packageBodyMaxSize + _packageCoder.GetPackageHeaderSize();

			// 创建字节缓冲类
			// 注意：字节缓冲区长度，推荐4倍最大包体长度
			int byteBufferSize = _packageMaxSize * 4;
			int tempBufferSize = _packageMaxSize * 2;
			_sendBuffer = new ByteBuffer(byteBufferSize);
			_receiveBuffer = new ByteBuffer(byteBufferSize);
			_tempBuffer = new byte[tempBufferSize];

			// 创建IOCP接收类
			_receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
			_receiveArgs.SetBuffer(_tempBuffer, 0, _tempBuffer.Length);

			// 创建IOCP发送类
			_sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
			_sendArgs.SetBuffer(_sendBuffer.GetBuffer(), 0, _sendBuffer.Capacity);

			// 请求操作
			bool willRaiseEvent = _socket.ReceiveAsync(_receiveArgs);
			if (!willRaiseEvent)
			{
				ProcessReceive(_receiveArgs);
			}

			// 发送线程
			ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadUpdate));
		}

		/// <summary>
		/// 检测Socket是否已连接
		/// </summary>
		public bool IsConnected()
		{
			if (_socket == null)
				return false;
			return _socket.Connected;
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			try
			{
				if (_socket != null)
					_socket.Shutdown(SocketShutdown.Both);

				_receiveArgs.Dispose();
				_sendArgs.Dispose();

				_sendQueue.Clear();
				_receiveQueue.Clear();
				_decodeTempList.Clear();

				_sendBuffer.Clear();
				_receiveBuffer.Clear();
			}
			catch (Exception)
			{
				// throws if client process has already closed, so it is not necessary to catch.
			}
			finally
			{
				if (_socket != null)
				{
					_socket.Close();
					_socket = null;
				}
			}
		}

		/// <summary>
		/// 主线程内更新
		/// </summary>
		public void Update()
		{
		}

		/// <summary>
		/// 子线程内发送数据
		/// </summary>
		private void ThreadUpdate(object param)
		{
			while (true)
			{
				if (_socket == null || _socket.Connected == false)
					return;

				if (_sendQueue.Count == 0)
				{
					Thread.Sleep(10);
				}

				lock (_sendQueue)
				{
					SendInternal();
				}
			}
		}
		private void SendInternal()
		{
			// 清空缓存
			_sendBuffer.Clear();

			// 合并数据一起发送
			while (_sendQueue.Count > 0)
			{
				// 如果不够写入一个最大的消息包
				if (_sendBuffer.WriteableBytes < _packageMaxSize)
					break;

				// 数据压码
				System.Object packet = _sendQueue.Dequeue();
				_packageCoder.Encode(_sendBuffer, packet);
			}

			// 请求操作
			_sendArgs.SetBuffer(0, _sendBuffer.ReadableBytes);
			bool willRaiseEvent = _socket.SendAsync(_sendArgs);
			if (!willRaiseEvent)
			{
				ProcessSend(_sendArgs);
			}
		}


		/// <summary>
		/// 发送网络包
		/// </summary>
		public void SendPackage(System.Object packet)
		{
			lock (_sendQueue)
			{
				_sendQueue.Enqueue(packet);
			}
		}

		/// <summary>
		/// 获取网络包
		/// </summary>
		public System.Object PickPackage()
		{
			System.Object package = null;
			lock (_receiveQueue)
			{
				if (_receiveQueue.Count > 0)
					package = _receiveQueue.Dequeue();
			}
			return package;
		}


		/// <summary>
		/// This method is called whenever a receive or send operation is completed on a socket 
		/// </summary>
		private void IO_Completed(object sender, SocketAsyncEventArgs e)
		{
			// determine which type of operation just completed and call the associated handler
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Receive:
					ProcessReceive(e);
					break;
				case SocketAsyncOperation.Send:
					ProcessSend(e);
					break;
				default:
					throw new ArgumentException("The last operation completed on the socket was not a receive or send");
			}
		}

		/// <summary>
		/// 数据接收完成时
		/// </summary>
		private void ProcessReceive(object obj)
		{
			SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;

			// check if the remote host closed the connection	
			if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
			{
				// 如果数据写穿
				if (_receiveBuffer.IsWriteable(e.BytesTransferred) == false)
				{
					HandleError(true, "The channel fatal error");
					return;
				}

				// 拷贝数据
				_receiveBuffer.WriteBytes(e.Buffer, 0, e.BytesTransferred);

				// 数据解码
				_decodeTempList.Clear();
				_packageCoder.Decode(_receiveBuffer, _decodeTempList);
				lock (_receiveQueue)
				{
					for (int i = 0; i < _decodeTempList.Count; i++)
					{
						_receiveQueue.Enqueue(_decodeTempList[i]);
					}
				}

				// 为接收下一段数据，投递接收请求
				e.SetBuffer(0, _tempBuffer.Length);
				bool willRaiseEvent = _socket.ReceiveAsync(e);
				if (!willRaiseEvent)
				{
					ProcessReceive(e);
				}
			}
			else
			{
				HandleError(true, $"ProcessReceive error : {e.SocketError}");
			}
		}

		/// <summary>
		/// 数据发送完成时
		/// </summary>
		private void ProcessSend(object obj)
		{
			SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
			if (e.SocketError == SocketError.Success)
			{
			}
			else
			{
				HandleError(true, $"ProcessSend error : {e.SocketError}");
			}
		}

		/// <summary>
		/// 捕获异常错误
		/// </summary>
		public void HandleError(bool isDispose, string error)
		{
			MotionLog.Error(error);
			if (isDispose) Dispose();
		}
	}
}