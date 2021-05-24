//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Patch
{
	public class RemoteServerInfo
	{
		private class ServerWrapper
		{
			public string Server;
			public string FallbackServer;
			public ServerWrapper(string server, string fallbackServer)
			{
				Server = server;
				FallbackServer = fallbackServer;
			}
		}

		/// <summary>
		/// WEB服务器地址
		/// </summary>
		private readonly Dictionary<int, string> _webServers = new Dictionary<int, string>();

		/// <summary>
		/// CDN服务器地址
		/// </summary>
		private readonly Dictionary<int, ServerWrapper> _cdnServers = new Dictionary<int, ServerWrapper>();

		/// <summary>
		/// 默认的Web服务器地址
		/// </summary>
		public string DefaultWebServer { private set; get; }

		/// <summary>
		/// 默认的CDN服务器地址
		/// </summary>
		public string DefaultCDNServer { private set; get; }


		public RemoteServerInfo(string defaultWebServer, string defaultCDNServer)
		{
			if (defaultWebServer.ToLower().StartsWith("http") == false)
				defaultWebServer = $"http://{defaultWebServer}";
			if (defaultCDNServer.ToLower().StartsWith("http") == false)
				defaultCDNServer = $"http://{defaultCDNServer}";

			DefaultWebServer = defaultWebServer;
			DefaultCDNServer = defaultCDNServer;
		}

		/// <summary>
		/// 添加服务器信息
		/// </summary>
		public void AddServerInfo(RuntimePlatform platform, string webServer, string cdnServer, string cdnFallbackServer)
		{
			if (string.IsNullOrEmpty(webServer))
				throw new Exception("Web server is null or empty.");
			if (string.IsNullOrEmpty(cdnServer))
				throw new Exception("CDN server is null or empty.");
			if (string.IsNullOrEmpty(cdnFallbackServer))
				throw new Exception("CDN fallback server is null or empty.");

			if (webServer.ToLower().StartsWith("http") == false)
				webServer = $"http://{webServer}";
			if (cdnServer.ToLower().StartsWith("http") == false)
				cdnServer = $"http://{cdnServer}";
			if (cdnFallbackServer.ToLower().StartsWith("http") == false)
				cdnFallbackServer = $"http://{cdnFallbackServer}";

			_webServers.Add((int)platform, webServer);
			_cdnServers.Add((int)platform, new ServerWrapper(cdnServer, cdnFallbackServer));
		}

		/// <summary>
		/// 获取Web服务器地址
		/// 注意：在没有相关平台的信息时，返回默认的Web服务器地址
		/// </summary>
		public string GetWebServer(RuntimePlatform platform)
		{
			if (_webServers.TryGetValue((int)platform, out string value))
			{
				return value;
			}
			return DefaultWebServer;
		}

		/// <summary>
		/// 获取CDN服务器地址
		/// 注意：在没有相关平台的信息时，返回默认的CDN服务器地址
		/// </summary>
		public string GetCDNServer(RuntimePlatform platform)
		{
			if (_cdnServers.TryGetValue((int)platform, out ServerWrapper value))
			{
				return value.Server;
			}
			return DefaultCDNServer;
		}

		/// <summary>
		/// 获取CDN服务器备用地址
		/// 注意：在没有相关平台的信息时，返回默认的CDN服务器地址
		/// </summary>
		public string GetCDNFallbackServer(RuntimePlatform platform)
		{
			if (_cdnServers.TryGetValue((int)platform, out ServerWrapper value))
			{
				return value.FallbackServer;
			}
			return DefaultCDNServer;
		}
	}
}