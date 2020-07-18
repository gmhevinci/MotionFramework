//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
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
		/// <summary>
		/// WEB服务器地址
		/// </summary>
		private readonly Dictionary<RuntimePlatform, string> _webServers = new Dictionary<RuntimePlatform, string>();

		/// <summary>
		/// CDN服务器地址
		/// </summary>
		private readonly Dictionary<RuntimePlatform, string> _cdnServers = new Dictionary<RuntimePlatform, string>();

		/// <summary>
		/// 默认的Web服务器地址
		/// </summary>
		public string DefaultWebServerIP { private set; get; }

		/// <summary>
		/// 默认的CDN服务器地址
		/// </summary>
		public string DefaultCDNServerIP { private set; get; }


		public RemoteServerInfo(string defaultWebServerIP, string defaultCDNServerIP)
		{
			if (defaultWebServerIP.ToLower().StartsWith("http") == false)
				defaultWebServerIP = $"http://{defaultWebServerIP}";
			if (defaultCDNServerIP.ToLower().StartsWith("http") == false)
				defaultCDNServerIP = $"http://{defaultCDNServerIP}";

			DefaultWebServerIP = defaultWebServerIP;
			DefaultCDNServerIP = defaultCDNServerIP;
		}

		/// <summary>
		/// 添加服务器信息
		/// </summary>
		public void AddServerInfo(RuntimePlatform platform, string webServerIP, string cdnServerIP)
		{
			if (string.IsNullOrEmpty(webServerIP))
				throw new Exception("Web server ip is null or empty.");
			if (string.IsNullOrEmpty(cdnServerIP))
				throw new Exception("CDN server ip is null or empty.");

			if (webServerIP.ToLower().StartsWith("http") == false)
				webServerIP = $"http://{webServerIP}";
			if (cdnServerIP.ToLower().StartsWith("http") == false)
				cdnServerIP = $"http://{cdnServerIP}";

			_webServers.Add(platform, webServerIP);
			_cdnServers.Add(platform, cdnServerIP);
		}

		/// <summary>
		/// 获取WEB服务器地址
		/// </summary>
		public string GetPlatformWebServerIP(RuntimePlatform platform)
		{
			if(_webServers.TryGetValue(platform, out string value))
			{
				return value;
			}
			return DefaultWebServerIP;
		}
		
		/// <summary>
		/// 获取CDN服务器地址
		/// </summary>
		public string GetPlatformCDNServerIP(RuntimePlatform platform)
		{
			if (_cdnServers.TryGetValue(platform, out string value))
			{
				return value;
			}
			return DefaultCDNServerIP;
		}
	}
}