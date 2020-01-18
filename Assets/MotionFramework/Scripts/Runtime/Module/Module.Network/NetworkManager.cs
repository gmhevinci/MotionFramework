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
using MotionFramework.Console;

namespace MotionFramework.Network
{
	/// <summary>
	/// 网络管理器
	/// </summary>
	public sealed class NetworkManager : ModuleSingleton<NetworkManager>, IModule
	{
		/// <summary>
		/// 游戏模块创建参数
		/// </summary>
		public class CreateParameters
		{
			/// <summary>
			/// 网络包编码解码器
			/// </summary>
			public System.Type PackageCoderType;
		}


		private TcpServer _server;
		private TcpChannel _channel;
		private System.Type _packageCoderType;

		// GUI显示数据
		private string _host;
		private int _port;
		private AddressFamily _family = AddressFamily.Unknown;

		/// <summary>
		/// 当前的网络状态
		/// </summary>
		public ENetworkStates States { private set; get; } = ENetworkStates.Disconnect;

		/// <summary>
		/// Mono层网络消息接收回调
		/// </summary>
		public Action<INetworkPackage> MonoPackageCallback;

		/// <summary>
		/// 热更层网络消息接收回调
		/// </summary>
		public Action<INetworkPackage> HotfixPackageCallback;


		void IModule.OnCreate(System.Object param)
		{
			CreateParameters createParam = param as CreateParameters;
			if (createParam == null)
				throw new Exception($"{nameof(NetworkManager)} create param is invalid.");

			_packageCoderType = createParam.PackageCoderType;
			_server = new TcpServer();
			_server.Start(false, null);
		}
		void IModule.OnUpdate()
		{
			_server.Update();

			if (_channel != null)
			{
				// 拉取网络包
				// 注意：如果服务器意外断开，未拉取的网络包将会丢失
				INetworkPackage package = (INetworkPackage)_channel.PickPackage();
				if (package != null)
				{
					if (package.IsHotfixPackage)
						HotfixPackageCallback.Invoke(package);
					else
						MonoPackageCallback.Invoke(package);
				}

				// 侦测服务器主动断开连接
				if (States == ENetworkStates.Connected)
				{
					if (_channel.IsConnected() == false)
					{
						States = ENetworkStates.Disconnect;
						NetworkEventDispatcher.SendDisconnectMsg();
						CloseChannel();
						MotionLog.Log(ELogLevel.Warning, "Server disconnect.");
					}
				}
			}
		}
		void IModule.OnGUI()
		{
			ConsoleGUI.Lable($"[{nameof(NetworkManager)}] State : {States}");
			ConsoleGUI.Lable($"[{nameof(NetworkManager)}] IP Host : {_host}");
			ConsoleGUI.Lable($"[{nameof(NetworkManager)}] IP Port : {_port}");
			ConsoleGUI.Lable($"[{nameof(NetworkManager)}] IP Type : {_family}");
		}

		/// <summary>
		/// 连接服务器
		/// </summary>
		/// <param name="host">地址</param>
		/// <param name="port">端口</param>
		public void ConnectServer(string host, int port)
		{
			if (States == ENetworkStates.Disconnect)
			{
				States = ENetworkStates.Connecting;
				NetworkEventDispatcher.SendBeginConnectMsg();
				IPEndPoint remote = new IPEndPoint(IPAddress.Parse(host), port);
				_server.ConnectAsync(remote, OnConnectServer, _packageCoderType);

				// 记录数据
				_host = host;
				_port = port;
				_family = remote.AddressFamily;
			}
		}
		private void OnConnectServer(TcpChannel channel, SocketError error)
		{
			MotionLog.Log(ELogLevel.Log, $"Server connect result : {error}");
			if (error == SocketError.Success)
			{
				_channel = channel;
				States = ENetworkStates.Connected;
				NetworkEventDispatcher.SendConnectSuccessMsg();
			}
			else
			{
				States = ENetworkStates.Disconnect;
				NetworkEventDispatcher.SendConnectFailMsg(error.ToString());
			}
		}

		/// <summary>
		/// 断开连接
		/// </summary>
		public void DisconnectServer()
		{
			if (States == ENetworkStates.Connected)
			{
				States = ENetworkStates.Disconnect;
				NetworkEventDispatcher.SendDisconnectMsg();
				CloseChannel();
			}
		}

		/// <summary>
		/// 发送网络消息
		/// </summary>
		public void SendMessage(INetworkPackage package)
		{
			if (States != ENetworkStates.Connected)
			{
				MotionLog.Log(ELogLevel.Warning, "Network is not connected.");
				return;
			}

			if (_channel != null)
				_channel.SendPackage(package);
		}

		private void CloseChannel()
		{
			if (_channel != null)
			{
				_server.CloseChannel(_channel);
				_channel = null;
			}
		}
	}
}