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

		// 版本号
		public Version AppVersion { private set; get; }
		public Version RequestedGameVersion { private set; get; }
		public int RequestedResourceVersion { private set; get; }
		
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


		public void Create(PatchManager.CreateParameters createParam)
		{
			_serverID = createParam.ServerID;
			_channelID = createParam.ChannelID;
			_deviceID = createParam.DeviceID;
			_testFlag = createParam.TestFlag;
			_checkLevel = createParam.CheckLevel;
			_serverInfo = createParam.ServerInfo;
			AppVersion = new Version(Application.version);
		}
		public void Download()
		{
			// 注意：按照先后顺序添加流程节点
			_procedure.AddNode(new FsmRequestGameVersion(this));
			_procedure.AddNode(new FsmParseWebPatchManifest(this));
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
				if (message.operation == EPatchOperation.BeginingDownloadWebFiles)
				{
					// 从挂起的地方继续
					if (_procedure.Current == EPatchStates.GetDonwloadList.ToString())
						_procedure.SwitchNext();
					else
						MotionLog.Error($"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryRequestGameVersion)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchStates.RequestGameVersion.ToString())
						_procedure.Switch(_procedure.Current);
					else
						MotionLog.Error($"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryDownloadWebPatchManifest)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchStates.ParseWebPatchManifest.ToString())
						_procedure.Switch(_procedure.Current);
					else
						MotionLog.Error($"Patch states is incorrect : {_procedure.Current}");
				}
				else if (message.operation == EPatchOperation.TryDownloadWebFiles)
				{
					// 修复当前节点错误
					if (_procedure.Current == EPatchStates.DownloadWebFiles.ToString())
						_procedure.Switch(EPatchStates.GetDonwloadList.ToString());
					else
						MotionLog.Error($"Patch states is incorrect : {_procedure.Current}");
				}
				else
				{
					throw new NotImplementedException($"{message.operation}");
				}
			}
		}

		/// <summary>
		/// 检测补丁文件有效性
		/// </summary>
		public bool CheckPatchFileValid(PatchElement element)
		{
			string filePath = AssetPathHelper.MakePersistentLoadPath(element.MD5);
			if (File.Exists(filePath) == false)
				return false;

			// 校验沙盒里的补丁文件
			if (_checkLevel == ECheckLevel.CheckSize)
			{
				long fileSize = FileUtility.GetFileSize(filePath);
				if (fileSize == element.SizeBytes)
					return true;
			}
			else if (_checkLevel == ECheckLevel.CheckMD5)
			{
				string md5 = HashUtility.FileMD5(filePath);
				if (md5 == element.MD5)
					return true;
			}
			else
			{
				throw new NotImplementedException(_checkLevel.ToString());
			}
			return false;
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

		// 补丁清单相关
		public void ParseAppPatchManifest(string jsonData)
		{
			if (AppPatchManifest != null)
				throw new Exception("Should never get here.");
			AppPatchManifest = PatchManifest.Deserialize(jsonData);
		}
		public void ParseSandboxPatchManifest(string jsonData)
		{
			if (SandboxPatchManifest != null)
				throw new Exception("Should never get here.");
			SandboxPatchManifest = PatchManifest.Deserialize(jsonData);
		}
		public void ParseSandboxPatchManifest(PatchManifest patchFile)
		{
			if (SandboxPatchManifest != null)
				throw new Exception("Should never get here.");
			SandboxPatchManifest = patchFile;
		}
		public void ParseWebPatchManifest(string jsonData)
		{
			if (WebPatchManifest != null)
				throw new Exception("Should never get here.");
			WebPatchManifest = PatchManifest.Deserialize(jsonData);
		}
		public void SaveWebPatchManifest()
		{
			if (WebPatchManifest == null)
				throw new Exception("WebPatchManifest is null.");

			// 注意：这里会覆盖掉沙盒内的旧文件
			string savePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestFileName);
			PatchManifest.Serialize(savePath, WebPatchManifest);
		}

		// 服务器IP相关
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
			RequestedGameVersion = new Version(response.GameVersion);
			RequestedResourceVersion = response.ResourceVersion;
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
			public int ResourceVersion; //当前资源版本
			public bool ForceInstall; //是否需要强制安装
			public string AppURL; //App安装的地址
#pragma warning restore 0649
		}
	}
}