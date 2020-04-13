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
		private Socket _listenSocket;
		private Type _packageCoderType;
		private int _packageMaxSize;

		/// <summary>
		/// 信号量
		/// This Semaphore is used to keep from going over max connection.
		/// </summary>
		private Semaphore _maxAcceptedSemaphore;

		/// <summary>
		/// 通信频道列表
		/// </summary>
		private readonly List<TcpChannel> _allChannels = new List<TcpChannel>(9999);


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


		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="listenPort">监听的端口</param>
		/// <param name="maxClient">最大的客户端数量</param>
		/// <param name="packageCoderType">监听频道使用的网络包编码解码器类型</param>
		/// <param name="packageMaxSize">网络包体的最大长度</param> 
		public TcpServer(int listenPort, int maxClient, Type packageCoderType, int packageMaxSize)
				: this(IPAddress.Any, listenPort, maxClient, packageCoderType, packageMaxSize)
		{
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="localEP">监听的终结点</param>
		/// <param name="maxClient">最大客户端数量</param>
		/// <param name="packageCoderType">监听频道使用的网络包编码解码器类型</param>
		/// <param name="packageMaxSize">网络包体的最大长度</param>
		public TcpServer(IPEndPoint localEP, int maxClient, Type packageCoderType, int packageMaxSize)
			: this(localEP.Address, localEP.Port, maxClient, packageCoderType, packageMaxSize)
		{
		}

		/// <summary>
		/// 构造函数
		/// </summary>
		private TcpServer(IPAddress address, int port, int maxClient, Type packageCoderType, int packageMaxSize)
		{
			Address = address;
			Port = port;
			MaxClient = maxClient;
			_packageCoderType = packageCoderType;
			_packageMaxSize = packageMaxSize;
		}


		/// <summary>
		/// 开始网络服务
		/// </summary>
		public void Start()
		{
			if (IsRunning)
				return;

			IsRunning = true;

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
		private void RemoveChannel(TcpChannel channel)
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
				channel.InitChannel(e.AcceptSocket, _packageCoderType, _packageMaxSize);

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

		private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation)
			{
				case SocketAsyncOperation.Accept:
					MainThreadSyncContext.Instance.Post(ProcessAccept, e);
					break;
				default:
					throw new ArgumentException("The last operation completed on the socket was not a accept");
			}
		}
	}
}