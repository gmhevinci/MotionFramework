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

namespace MotionFramework.Network
{
	public class TcpChannel : IDisposable
	{
		#region Fields
		private readonly SocketAsyncEventArgs _receiveArgs = new SocketAsyncEventArgs();
		private readonly SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

		private readonly Queue<System.Object> _sendQueue = new Queue<System.Object>(10000);
		private readonly Queue<System.Object> _receiveQueue = new Queue<System.Object>(10000);
		private readonly List<System.Object> _decodeTempList = new List<object>(100);

		private NetworkPackageCoder _packageCoder = null;

		private bool _isSending = false;
		private bool _isReceiving = false;
		#endregion

		#region Properties
		/// <summary>
		/// 通信Socket
		/// </summary>
		private Socket IOSocket { get; set; }

		/// <summary>
		/// 频道是否有效
		/// </summary>
		public bool IsValid { get { return IOSocket != null; } }
		#endregion


		/// <summary>
		/// 初始化频道
		/// </summary>
		public void InitChannel(Socket socket, Type packageCoderType)
		{
			IOSocket = socket;
			IOSocket.NoDelay = true;

			// 创建编码解码器
			_packageCoder = (NetworkPackageCoder)Activator.CreateInstance(packageCoderType);
			_packageCoder.InitChannel(this);

			_receiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
			_receiveArgs.SetBuffer(_packageCoder.GetReceiveBuffer(), 0, _packageCoder.GetReceiveBufferCapacity());

			_sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
			_sendArgs.SetBuffer(_packageCoder.GetSendBuffer(), 0, _packageCoder.GetSendBufferCapacity());
		}

		/// <summary>
		/// 检测Socket是否已连接
		/// </summary>
		public bool IsConnected()
		{
			if (IOSocket == null)
				return false;
			return IOSocket.Connected;
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			try
			{
				if (IOSocket != null)
					IOSocket.Shutdown(SocketShutdown.Both);

				_receiveArgs.Dispose();
				_sendArgs.Dispose();

				_sendQueue.Clear();
				_receiveQueue.Clear();
				_decodeTempList.Clear();

				if (_packageCoder != null)
					_packageCoder.Dispose();

				_isSending = false;
				_isReceiving = false;
			}
			catch (Exception)
			{
				// throws if client process has already closed, so it is not necessary to catch.
			}
			finally
			{
				if (IOSocket != null)
				{
					IOSocket.Close();
					IOSocket = null;
				}
			}
		}

		/// <summary>
		/// 主线程内更新
		/// </summary>
		public void Update()
		{
			if (IOSocket == null || IOSocket.Connected == false)
				return;

			// 接收数据
			if (_isReceiving == false)
			{
				_isReceiving = true;

				// 清空缓存
				_packageCoder.ClearReceiveBuffer();

				// 请求操作
				_receiveArgs.SetBuffer(0, _packageCoder.GetReceiveBufferCapacity());
				bool willRaiseEvent = IOSocket.ReceiveAsync(_receiveArgs);
				if (!willRaiseEvent)
				{
					ProcessReceive(_receiveArgs);
				}
			}

			// 发送数据
			if (_isSending == false && _sendQueue.Count > 0)
			{
				_isSending = true;

				// 清空缓存
				_packageCoder.ClearSendBuffer();

				// 合并数据一起发送
				while (_sendQueue.Count > 0)
				{
					// 数据压码
					System.Object packet = _sendQueue.Dequeue();
					_packageCoder.Encode(packet);

					// 如果已经超过一个最大包体尺寸
					// 注意：发送的数据理论最大值为俩个最大包体大小
					if (_packageCoder.GetSendBufferWriterIndex() >= NetworkDefine.PackageBodyMaxSize)
						break;
				}

				// 请求操作
				_sendArgs.SetBuffer(0, _packageCoder.GetSendBufferReadableBytes());
				bool willRaiseEvent = IOSocket.SendAsync(_sendArgs);
				if (!willRaiseEvent)
				{
					ProcessSend(_sendArgs);
				}
			}
		}


		/// <summary>
		/// 发送网络包
		/// </summary>
		public void SendPackage(System.Object packet)
		{
			_sendQueue.Enqueue(packet);
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
					MainThreadSyncContext.Instance.Post(ProcessReceive, e);
					break;
				case SocketAsyncOperation.Send:
					MainThreadSyncContext.Instance.Post(ProcessSend, e);
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
				_packageCoder.SetReceiveDataSize(e.BytesTransferred);

				// 如果数据写穿
				if (_packageCoder.GetReceiveBufferWriterIndex() > _packageCoder.GetReceiveBufferCapacity())
				{
					HandleError(true, "The channel fatal error");
					return;
				}

				// 数据解码
				_decodeTempList.Clear();
				_packageCoder.Decode(_decodeTempList);
				lock (_receiveQueue)
				{
					for(int i=0; i< _decodeTempList.Count; i++)
					{
						_receiveQueue.Enqueue(_decodeTempList[i]);
					}
				}

				// 为接收下一段数据，投递接收请求
				e.SetBuffer(_packageCoder.GetReceiveBufferWriterIndex(), _packageCoder.GetReceiveBufferWriteableBytes());
				bool willRaiseEvent = IOSocket.ReceiveAsync(e);
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
				_isSending = false;
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
			MotionLog.Log(ELogLevel.Error, error);
			if (isDispose) Dispose();
		}
	}
}