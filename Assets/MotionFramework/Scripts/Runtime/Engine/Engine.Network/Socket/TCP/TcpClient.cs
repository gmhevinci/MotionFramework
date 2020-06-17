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

namespace MotionFramework.Network
{
	/// <summary>
	/// 异步IOCP SOCKET客户端
	/// </summary>
	public class TcpClient : IDisposable
	{
		private class UserToken
		{
			public System.Action<SocketError> Callback;
		}

		private TcpChannel _channel;
		private Type _packageCoderType;
		private int _packageMaxSize;


		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="packageCoderType">通信频道使用的网络包编码解码器类型</param>
		/// <param name="packageMaxSize">网络包最大长度</param>
		public TcpClient(Type packageCoderType, int packageMaxSize)
		{
			_packageCoderType = packageCoderType;
			_packageMaxSize = packageMaxSize;
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if(_channel != null)
			{
				_channel.Dispose();
				_channel = null;
			}
		}

		/// <summary>
		/// 更新网络
		/// </summary>
		public void Update()
		{
			MainThreadSyncContext.Instance.Update();

			if (_channel != null)
				_channel.Update();
		}


		/// <summary>
		/// 发送网络包
		/// </summary>
		public void SendPackage(System.Object packet)
		{
			if(_channel != null)
				_channel.SendPackage(packet);
		}

		/// <summary>
		/// 获取网络包
		/// </summary>
		public System.Object PickPackage()
		{
			if (_channel == null)
				return null;

			return _channel.PickPackage();
		}

		/// <summary>
		/// 检测Socket是否已连接
		/// </summary>
		public bool IsConnected()
		{
			if (_channel == null)
				return false;

			return _channel.IsConnected();
		}


		/// <summary>
		/// 异步连接
		/// </summary>
		/// <param name="remote">IP终端</param>
		/// <param name="callback">连接回调</param>
		public void ConnectAsync(IPEndPoint remote, System.Action<SocketError> callback)
		{
			UserToken token = new UserToken()
			{
				Callback = callback,
			};

			SocketAsyncEventArgs args = new SocketAsyncEventArgs();
			args.RemoteEndPoint = remote;
			args.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
			args.UserToken = token;

			Socket clientSock = new Socket(remote.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			bool willRaiseEvent = clientSock.ConnectAsync(args);
			if (!willRaiseEvent)
			{
				ProcessConnected(args);
			}
		}

		/// <summary>
		/// 处理连接请求
		/// </summary>
		private void ProcessConnected(object obj)
		{
			SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
			UserToken token = (UserToken)e.UserToken;
			if (e.SocketError == SocketError.Success)
			{
				if (_channel != null)
					throw new Exception("TcpClient channel is created.");

				// 创建频道
				_channel = new TcpChannel();
				_channel.InitChannel(e.ConnectSocket, _packageCoderType, _packageMaxSize);
			}
			else
			{
				MotionLog.Error($"ProcessConnected error : {e.SocketError}");
			}

			// 回调函数		
			if (token.Callback != null)
				token.Callback.Invoke(e.SocketError);
		}

		private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Connect:
					MainThreadSyncContext.Instance.Post(ProcessConnected, e);
					break;
				default:
					throw new ArgumentException("The last operation completed on the socket was not a connect");
			}
		}
	}
}
