//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Copyright©2020-2020 ZensYue
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MotionFramework.AI;
using MotionFramework.Event;
using MotionFramework.Resource;
using MotionFramework.Utility;
using MotionFramework.IO;

namespace MotionFramework.Patch
{
	internal class PatchManagerImpl
	{
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
			public int ResourceVersion; //当前资源版本
			public bool ForceInstall; //是否需要强制安装
			public string AppURL; //App安装的地址
#pragma warning restore 0649
		}

		private readonly ProcedureFsm _procedure = new ProcedureFsm();
		
		// 参数相关
		private int _serverID;
		private int _channelID;
		private long _deviceID;
		private int _testFlag;
		private ECheckLevel _checkLevel;
		private RemoteServerInfo _serverInfo;

		// 强更标记和地址
		public bool ForceInstall { private set; get; } = false;
		public string AppURL { private set; get; }

		// 请求的版本号
		public Version RequestedGameVersion { private set; get; }
		public int RequestedResourceVersion { private set; get; }

		// 下载相关
		public List<PatchElement> DownloadList;

		/// <summary>
		/// 缓存系统
		/// </summary>
		public PatchCache Cache { private set; get; }

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


		public void Create(PatchManager.CreateParameters createParam)
		{
			_serverID = createParam.ServerID;
			_channelID = createParam.ChannelID;
			_deviceID = createParam.DeviceID;
			_testFlag = createParam.TestFlag;
			_checkLevel = createParam.CheckLevel;
			_serverInfo = createParam.ServerInfo;
			Cache = new PatchCache(this, _checkLevel);
		}
		public void Download()
		{
			// 注意：按照先后顺序添加流程节点
			_procedure.AddNode(new FsmRequestGameVersion(this));
			_procedure.AddNode(new FsmGetWebPatchManifest(this));
			_procedure.AddNode(new FsmGetDonwloadList(this));
			_procedure.AddNode(new FsmDownloadWebFiles(this));
			_procedure.AddNode(new FsmDownloadOver(this));
			_procedure.Run();
		}
		public void Update()
		{
			_procedure.Update();
		}

		/// <summary>
		/// 清空缓存
		/// </summary>
		public void ClearCache()
		{
			Cache.ClearCache();
		}

		/// <summary>
		/// 接收事件
		/// </summary>
		public void HandleEventMessage(IEventMessage msg)
		{
			if (msg is PatchEventMessageDefine.OperationEvent)
			{
				var message = msg as PatchEventMessageDefine.OperationEvent;
				if (message.operation == EPatchOperation.BeginingDownloadWebFiles)
				{
					// 从挂起的地方继续
					if (_procedure.Current == EPatchSteps.GetDonwloadList.ToString())
						_procedure.SwitchNext();
					else
						MotionLog.Error($"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryRequestGameVersion)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchSteps.RequestGameVersion.ToString())
						_procedure.Switch(_procedure.Current);
					else
						MotionLog.Error($"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryDownloadWebPatchManifest)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchSteps.GetWebPatchManifest.ToString())
						_procedure.Switch(_procedure.Current);
					else
						MotionLog.Error($"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryDownloadWebFiles)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchSteps.DownloadWebFiles.ToString())
						_procedure.Switch(EPatchSteps.GetDonwloadList.ToString());
					else
						MotionLog.Error($"Patch states is incorrect : {_procedure.Current}");
				}
				else
				{
					throw new NotImplementedException($"{message.operation}");
				}
			}
		}

		// 下载相关
		public void ClearDownloadList()
		{
			DownloadList.Clear();
		}
		public int GetDownloadTotalCount()
		{
			return DownloadList.Count;
		}
		public long GetDownloadTotalSize()
		{
			long totalDownloadSizeBytes = 0;
			foreach (var element in DownloadList)
			{
				totalDownloadSizeBytes += element.SizeBytes;
			}
			return totalDownloadSizeBytes;
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

		// 服务器地址相关
		public string GetWebServerIP()
		{
			RuntimePlatform runtimePlatform = Application.platform;
			return _serverInfo.GetPlatformWebServerIP(runtimePlatform);
		}
		public string GetCDNServerIP()
		{
			RuntimePlatform runtimePlatform = Application.platform;
			return _serverInfo.GetPlatformCDNServerIP(runtimePlatform);
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
				AppVersion = Application.version,
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
			RequestedGameVersion = new Version(response.GameVersion);
			RequestedResourceVersion = response.ResourceVersion;
			ForceInstall = response.ForceInstall;
			AppURL = response.AppURL;
		}
	}
}