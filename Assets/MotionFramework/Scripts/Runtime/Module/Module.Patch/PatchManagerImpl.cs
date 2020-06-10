//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.AI;
using MotionFramework.Event;

namespace MotionFramework.Patch
{
	internal class PatchManagerImpl
	{
		private readonly Procedure _procedure = new Procedure();

		// 参数相关
		private Dictionary<RuntimePlatform, string> _webServers;
		private Dictionary<RuntimePlatform, string> _cdnServers;
		private string _defaultWebServer;
		private string _defaultCDNServer;
		private int _serverID;
		private int _channelID;
		private long _deviceID;
		private int _testFlag;

		// 强更标记和地址
		public bool ForceInstall { private set; get; } = false;
		public string AppURL { private set; get; }

		// 校验等级
		public ECheckLevel CheckLevel { private set; get; }

		// 版本号
		public Version AppVersion { private set; get; }
		public Version GameVersion { private set; get; }

		// 补丁清单
		public PatchManifest AppPatchManifest { private set; get; }
		public PatchManifest SandboxPatchManifest { private set; get; }
		public PatchManifest WebPatchManifest { private set; get; }

		/// <summary>
		/// 下载列表
		/// </summary>
		public readonly List<PatchElement> DownloadList = new List<PatchElement>(1000);

		/// <summary>
		/// 当前运行的状态
		/// </summary>
		public string CurrentStates
		{
			get
			{
				return _procedure.Current;
			}
		}

		/// <summary>
		/// 向WEB服务器请求的资源版本号
		/// </summary>
		public int RequestedResourceVersion
		{
			get
			{
				if (GameVersion.Revision < 0)
					return 0;
				return GameVersion.Revision;
			}
		}


		public void Initialize(PatchManager.CreateParameters createParam)
		{
			_webServers = createParam.WebServers;
			_cdnServers = createParam.CDNServers;
			_defaultWebServer = createParam.DefaultWebServerIP;
			_defaultCDNServer = createParam.DefaultCDNServerIP;
			_serverID = createParam.ServerID;
			_channelID = createParam.ChannelID;
			_deviceID = createParam.DeviceID;
			_testFlag = createParam.TestFlag;
			CheckLevel = createParam.CheckLevel;
			AppVersion = new Version(Application.version);
		}
		public void Run()
		{
			// 注意：按照先后顺序添加流程节点
			_procedure.AddNode(new FsmInitiationBegin(this));
			_procedure.AddNode(new FsmCheckSandboxDirty(this));
			_procedure.AddNode(new FsmParseAppPatchManifest(this));
			_procedure.AddNode(new FsmParseSandboxPatchManifest(this));
			_procedure.AddNode(new FsmInitiationOver(this));
			_procedure.AddNode(new FsmRequestGameVersion(this));
			_procedure.AddNode(new FsmParseWebPatchManifest(this));
			_procedure.AddNode(new FsmGetDonwloadList(this));
			_procedure.AddNode(new FsmDownloadWebFiles(this));
			_procedure.AddNode(new FsmDownloadWebPatchManifest(this));
			_procedure.AddNode(new FsmDownloadOver(this));
			_procedure.Run();
		}
		public void Update()
		{
			_procedure.Update();
		}

		/// <summary>
		/// 修复客户端
		/// </summary>
		public void FixClient()
		{
			// 清空缓存
			PatchHelper.ClearSandbox();

			// 重启游戏
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		/// <summary>
		/// 接收事件
		/// </summary>
		public void HandleEventMessage(IEventMessage msg)
		{
			if (msg is PatchEventMessageDefine.OperationEvent)
			{
				var message = msg as PatchEventMessageDefine.OperationEvent;
				if (message.operation == EPatchOperation.BeginingRequestGameVersion)
				{
					// 从挂起的地方继续
					if (_procedure.Current == EPatchStates.InitiationOver.ToString())
						_procedure.SwitchNext();
					else
						MotionLog.Log(ELogLevel.Error, $"Patch system is not prepare : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.BeginingDownloadWebFiles)
				{
					// 从挂起的地方继续
					if (_procedure.Current == EPatchStates.GetDonwloadList.ToString())
						_procedure.SwitchNext();
					else
						MotionLog.Log(ELogLevel.Error, $"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryRequestGameVersion)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchStates.RequestGameVersion.ToString())
						_procedure.Switch(_procedure.Current);
					else
						MotionLog.Log(ELogLevel.Error, $"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryDownloadWebPatchManifest)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchStates.DownloadWebPatchManifest.ToString() || _procedure.Current == EPatchStates.ParseWebPatchManifest.ToString())
						_procedure.Switch(_procedure.Current);
					else
						MotionLog.Log(ELogLevel.Error, $"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryDownloadWebFiles)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchStates.DownloadWebFiles.ToString())
						_procedure.Switch(EPatchStates.GetDonwloadList.ToString());
					else
						MotionLog.Log(ELogLevel.Error, $"Patch states is incorrect : {_procedure.Current}");
				}
				else
				{
					throw new NotImplementedException($"{message.operation}");
				}
			}
		}

		// 流程相关
		public void Switch(string nodeName)
		{
			_procedure.Switch(nodeName);
		}
		public void SwitchNext()
		{
			_procedure.SwitchNext();
		}
		public void SwitchLast()
		{
			_procedure.SwitchLast();
		}

		// 解析补丁清单文件相关接口
		public void ParseAppPatchManifest(string fileContent)
		{
			if (AppPatchManifest != null)
				throw new Exception("Should never get here.");
			AppPatchManifest = new PatchManifest();
			AppPatchManifest.Parse(fileContent);
		}
		public void ParseSandboxPatchManifest(string fileContent)
		{
			if (SandboxPatchManifest != null)
				throw new Exception("Should never get here.");
			SandboxPatchManifest = new PatchManifest();
			SandboxPatchManifest.Parse(fileContent);
		}
		public void ParseSandboxPatchManifest(PatchManifest patchFile)
		{
			if (SandboxPatchManifest != null)
				throw new Exception("Should never get here.");
			SandboxPatchManifest = patchFile;
		}
		public void ParseWebPatchManifest(string fileContent)
		{
			if (WebPatchManifest != null)
				throw new Exception("Should never get here.");
			WebPatchManifest = new PatchManifest();
			WebPatchManifest.Parse(fileContent);
		}

		// 服务器IP相关
		public string GetWebServerIP()
		{
			RuntimePlatform runtimePlatform = Application.platform;
			if (_webServers != null && _webServers.ContainsKey(runtimePlatform))
				return _webServers[runtimePlatform];
			else
				return _defaultWebServer;
		}
		public string GetCDNServerIP()
		{
			RuntimePlatform runtimePlatform = Application.platform;
			if (_cdnServers != null && _cdnServers.ContainsKey(runtimePlatform))
				return _cdnServers[runtimePlatform];
			else
				return _defaultCDNServer;
		}

		// WEB相关
		public string GetWebDownloadURL(string resourceVersion, string fileName)
		{
			return $"{GetCDNServerIP()}/{resourceVersion}/{fileName}";
		}
		public string GetWebPostData()
		{
			WebPost post = new WebPost
			{
				AppVersion = AppVersion.ToString(),
				ServerID = _serverID,
				ChannelID = _channelID,
				DeviceID = _deviceID,
				TestFlag = _testFlag
			};
			return JsonUtility.ToJson(post);
		}
		public void ParseResponseData(string data)
		{
			if (string.IsNullOrEmpty(data))
				throw new Exception("Web server response data is null or empty.");

			WebResponse response = JsonUtility.FromJson<WebResponse>(data);
			GameVersion = new Version(response.GameVersion);
			ForceInstall = response.ForceInstall;
			AppURL = response.AppURL;
		}

		private class WebPost
		{
			public string AppVersion; //应用程序内置版本
			public int ServerID; //最近登录的服务器ID
			public int ChannelID; //渠道ID
			public long DeviceID; //设备唯一ID
			public int TestFlag; //测试标记
		}
		private class WebResponse
		{
#pragma warning disable 0649
			public string GameVersion; //当前游戏版本号
			public bool ForceInstall; //是否需要强制安装
			public string AppURL; //App安装的地址
#pragma warning restore 0649
		}
	}
}