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
	/// 异步IOCP SOCKET服务器
	/// </summary>
	public class TcpServer : IDisposable
	{
		#region Fields
		/// <summary>
		/// 监听频道使用的网络包编码解码器类型
		/// </summary>
		private Type _listenerPackageCoderType;

		/// <summary>
		/// 监听Socket，用于接受客户端的连接请求
		/// </summary>
		private Socket _listenSocket;

		/// <summary>
		/// 信号量
		/// This Semaphore is used to keep from going over max connection.
		/// </summary>
		private Semaphore _maxAcceptedSemaphore;

		/// <summary>
		/// 通信频道列表
		/// </summary>
		private readonly List<TcpChannel> _allChannels = new List<TcpChannel>(9999);
		#endregion

		#region Properties
		/// <summary>
		/// 服务器程序允许的最大客户端连接数
		/// </summary>
		public int MaxClient { get; }

		/// <summary>
		/// 服务器是否正在运行
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// 监听的IP地址
		/// </summary>
		public IPAddress Address { get; private set; }

		/// <summary>
		/// 监听的端口
		/// </summary>
		public int Port { get; private set; }
		#endregion

		#region Ctors
		/// <summary>
		/// 异步Socket TCP服务器
		/// </summary>
		/// <param name="listenPort">监听的端口</param>
		/// <param name="maxClient">最大的客户端数量</param>
		public TcpServer(int listenPort, int maxClient)
				: this(IPAddress.Any, listenPort, maxClient)
		{
		}

		/// <summary>
		/// 异步Socket TCP服务器
		/// </summary>
		/// <param name="localEP">监听的终结点</param>
		/// <param name="maxClient">最大客户端数量</param>
		public TcpServer(IPEndPoint localEP, int maxClient)
			: this(localEP.Address, localEP.Port, maxClient)
		{
		}

		/// <summary>
		/// 客户端 TCP服务器
		/// </summary>
		public TcpServer()
				: this(IPAddress.Any, 0, 0)
		{
		}

		private TcpServer(IPAddress address, int port, int maxClient)
		{
			Address = address;
			Port = port;
			MaxClient = maxClient;
		}
		#endregion

		/// <summary>
		/// 开始网络服务
		/// </summary>
		/// <param name="openListen">是否开启监听</param>
		/// <param name="listenerPackageCoderType">监听频道使用的网络包编码解码器类型</param>
		public void Start(bool openListen, Type listenerPackageCoderType)
		{
			if (IsRunning)
				return;

			IsRunning = true;

			// 解析器类型
			_listenerPackageCoderType = listenerPackageCoderType;

			// 如果需要开启监听
			if(openListen)
			{
				// 最大连接数信号
				_maxAcceptedSemaphore = new Semaphore(MaxClient, MaxClient);

				// 创建监听socket
				IPEndPoint localEndPoint = new IPEndPoint(Address, Port);
				_listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				_listenSocket.Bind(localEndPoint);

				// 开始监听
				_listenSocket.Listen(1000);

				// 在监听Socket上投递一个接受请求
				StartAccept(null);
			}
		}

		/// <summary>
		/// 停止网络服务
		/// </summary>
		public void Stop()
		{
			if (IsRunning)
			{
				IsRunning = false;
				Dispose();
			}
		}

		/// <summary>
		/// 更新
		/// </summary>
		public void Update()
		{
			MainThreadSyncContext.Instance.Update();

			for (int i = 0; i < _allChannels.Count; i++)
			{
				_allChannels[i].Update();
			}
		}

		/// <summary>
		/// 关闭频道并从列表里移除
		/// </summary>
		public void CloseChannel(TcpChannel channel)
		{
			if (channel == null)
				return;

			channel.Dispose();

			// 从频道列表里删除
			lock (_allChannels)
			{
				_allChannels.Remove(channel);
			}

			// 信号减弱
			if (_maxAcceptedSemaphore != null)
				_maxAcceptedSemaphore.Release();
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (_listenSocket != null)
			{
				_listenSocket.Close();
				_listenSocket = null;
			}

			if (_maxAcceptedSemaphore != null)
			{
				_maxAcceptedSemaphore.Close();
				_maxAcceptedSemaphore = null;
			}

			for (int i = 0; i < _allChannels.Count; i++)
			{
				_allChannels[i].Dispose();
			}
			_allChannels.Clear();
		}

		#region 监听连接
		/// <summary>
		/// 从客户端开始接受一个连接操作
		/// Begins an operation to accept a connection request from the client 
		/// </summary>
		private void StartAccept(SocketAsyncEventArgs acceptEventArg)
		{
			if (acceptEventArg == null)
			{
				acceptEventArg = new SocketAsyncEventArgs();
				acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
			}
			else
			{
				// socket must be cleared since the context object is being reused
				acceptEventArg.AcceptSocket = null;
			}

			// 查询等待是否有空闲socket资源
			if (_maxAcceptedSemaphore != null)
				_maxAcceptedSemaphore.WaitOne();

			// 开始处理请求
			bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
			if (!willRaiseEvent)
			{
				// IO没有挂起，操作同步完成
				ProcessAccept(acceptEventArg);
			}
		}

		/// <summary>
		/// 处理接收请求
		/// </summary>
		private void ProcessAccept(object obj)
		{
			SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
			if (e.SocketError == SocketError.Success)
			{
				// 创建频道
				TcpChannel channel = new TcpChannel();
				channel.InitChannel(e.AcceptSocket, _listenerPackageCoderType);

				// 加入到频道列表
				lock (_allChannels)
				{
					_allChannels.Add(channel);
				}
			}
			else
			{
				MotionLog.Log(ELogLevel.Error, $"ProcessAccept error : {e.SocketError}");
			}

			// 投递下一个接收请求
			StartAccept(e);
		}
		#endregion

		#region 主动连接
		private class UserToken
		{
			public System.Action<TcpChannel, SocketError> Callback;
			public System.Type PackageCoderType;
		}

		/// <summary>
		/// 异步连接
		/// </summary>
		/// <param name="remote">IP终端</param>
		/// <param name="callback">连接回调</param>
		/// <param name="packageCoderType">网络包编码解码器类型</param>
		public void ConnectAsync(IPEndPoint remote, System.Action<TcpChannel, SocketError> callback, System.Type packageCoderType)
		{
			if(IsRunning == false)
			{
				MotionLog.Log(ELogLevel.Warning, "Server is not start.");
				return;
			}

			UserToken token = new UserToken()
			{
				Callback = callback,
				PackageCoderType = packageCoderType,
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
			TcpChannel channel = null;
			SocketAsyncEventArgs e = obj as SocketAsyncEventArgs;
			UserToken token = (UserToken)e.UserToken;
			if (e.SocketError == SocketError.Success)
			{
				// 创建频道
				channel = new TcpChannel();
				channel.InitChannel(e.ConnectSocket, token.PackageCoderType);

				// 加入到频道列表
				lock (_allChannels)
				{
					_allChannels.Add(channel);
				}
			}
			else
			{
				MotionLog.Log(ELogLevel.Error, $"ProcessConnected error : {e.SocketError}");
			}

			// 回调函数		
			if (token.Callback != null)
				token.Callback.Invoke(channel, e.SocketError);
		}
		#endregion

		private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					MainThreadSyncContext.Instance.Post(ProcessAccept, e);
					break;
				case SocketAsyncOperation.Connect:
					MainThreadSyncContext.Instance.Post(ProcessConnected, e);
					break;
				default:
					throw new ArgumentException("The last operation completed on the socket was not a accept or connect");
			}
		}
	}
}