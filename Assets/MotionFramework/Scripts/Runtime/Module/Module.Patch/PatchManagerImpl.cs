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
using MotionFramework.Network;
using MotionFramework.Utility;

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

		// 强更标记和APP地址
		public bool ForceInstall { private set; get; } = false;
		public string AppURL { private set; get; }

		// 请求的版本号
		public Version RequestedGameVersion { private set; get; }
		public int RequestedResourceVersion { private set; get; }

		// 补丁清单
		private PatchManifest _appPatchManifest;
		private PatchManifest _localPatchManifest;
		private PatchManifest _remotePatchManifest;
		private CacheData _cache;

		// 补丁下载器
		public PatchDownloader Downloader;

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
		/// 本地资源版本号
		/// </summary>
		public int LocalResourceVersion
		{
			get
			{
				if (_localPatchManifest == null)
					return 0;
				return _localPatchManifest.ResourceVersion;
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
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAsync()
		{
			MotionLog.Log($"Beginning to initialize cache");

			// 加载缓存
			_cache = CacheData.LoadCache();

			// 检测沙盒被污染
			// 注意：在覆盖安装的时候，会保留沙盒目录里的文件，所以需要强制清空
			{
				// 如果是首次打开，记录APP版本号
				if (PatchHelper.CheckSandboxCacheFileExist() == false)
				{
					_cache.CacheVersion = Application.version;
					_cache.SaveCache();
				}
				else
				{
					// 每次启动时比对APP版本号是否一致	
					if (_cache.CacheVersion != Application.version)
					{
						MotionLog.Warning($"Cache is dirty ! Cache version is {_cache.CacheVersion}, APP version is {Application.version}");
						ClearCache();

						// 重新写入最新的APP版本号
						_cache.CacheVersion = Application.version;
						_cache.SaveCache();
					}
				}
			}

			// 加载APP内的补丁清单
			MotionLog.Log($"Load app patch file.");
			{
				string filePath = AssetPathHelper.MakeStreamingLoadPath(PatchDefine.PatchManifestFileName);
				string url = AssetPathHelper.ConvertToWWWPath(filePath);
				WebDataRequest downloader = new WebDataRequest(url);
				downloader.DownLoad();
				yield return downloader;

				if (downloader.HasError())
				{
					downloader.ReportError();
					downloader.Dispose();
					throw new System.Exception($"Fatal error : Failed download file : {url}");
				}

				string jsonData = downloader.GetText();
				_appPatchManifest = PatchManifest.Deserialize(jsonData);
				downloader.Dispose();
			}

			// 加载沙盒内的补丁清单
			MotionLog.Log($"Load sandbox patch file.");
			if (PatchHelper.CheckSandboxPatchManifestFileExist())
			{
				string filePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestFileName);
				string jsonData = File.ReadAllText(filePath);
				_localPatchManifest = PatchManifest.Deserialize(jsonData);
			}
			else
			{
				_localPatchManifest = _appPatchManifest;
			}
		}

		/// <summary>
		/// 开始补丁更新流程
		/// </summary>
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

		/// <summary>
		/// 更新流程
		/// </summary>
		public void Update()
		{
			_procedure.Update();
		}

		/// <summary>
		/// 清空缓存并删除所有沙盒文件
		/// </summary>
		public void ClearCache()
		{
			MotionLog.Warning("Clear cache and remove all sandbox files.");
			PatchHelper.ClearSandbox();
			_appPatchManifest = null;
			_localPatchManifest = null;
			_remotePatchManifest = null;
			_cache = null;
		}

		/// <summary>
		/// 获取本地补丁清单
		/// </summary>
		public PatchManifest GetPatchManifest()
		{
			return _localPatchManifest;
		}

		/// <summary>
		/// 获取AssetBundle的加载信息
		/// </summary>
		public AssetBundleInfo GetAssetBundleInfo(string manifestPath)
		{
			if (_localPatchManifest.Elements.TryGetValue(manifestPath, out PatchElement element))
			{
				// 查询内置资源
				if (_appPatchManifest.Elements.TryGetValue(manifestPath, out PatchElement appElement))
				{
					if (appElement.IsDLC() == false && appElement.MD5 == element.MD5)
					{
						string appLoadPath = AssetPathHelper.MakeStreamingLoadPath(manifestPath);
						AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, appLoadPath, string.Empty, appElement.Version, appElement.IsEncrypted);
						return bundleInfo;
					}
				}

				// 查询缓存资源
				// 注意：如果沙盒内缓存文件不存在，那么将会从服务器下载
				string sandboxLoadPath = PatchHelper.MakeSandboxCacheFilePath(element.MD5);
				if (_cache.Contains(element.MD5))
				{
					AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, sandboxLoadPath, string.Empty, element.Version, element.IsEncrypted);
					return bundleInfo;
				}
				else
				{
					string remoteURL = GetWebDownloadURL(element.Version.ToString(), element.Name);
					AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, sandboxLoadPath, remoteURL, element.Version, element.IsEncrypted);
					return bundleInfo;
				}
			}
			else
			{
				MotionLog.Warning($"Not found element in patch manifest : {manifestPath}");
				AssetBundleInfo bundleInfo = new AssetBundleInfo(manifestPath, string.Empty);
				return bundleInfo;
			}
		}

		/// <summary>
		/// 获取补丁的下载列表
		/// </summary>
		public List<PatchElement> GetPatchDownloadList()
		{
			List<PatchElement> downloadList = new List<PatchElement>(1000);

			// 准备下载列表
			foreach (var webElement in _remotePatchManifest.ElementList)
			{
				// 忽略DLC资源
				if (webElement.IsDLC())
					continue;

				// 内置资源比较
				if (_appPatchManifest.Elements.TryGetValue(webElement.Name, out PatchElement appElement))
				{
					if (appElement.IsDLC() == false && appElement.MD5 == webElement.MD5)
						continue;
				}

				// 缓存资源比较
				if (_cache.Contains(webElement.MD5))
					continue;

				downloadList.Add(webElement);
			}

			// 检测文件是否已经下载完毕
			// 注意：如果玩家在加载过程中强制退出，下次再进入的时候跳过已经加载的文件
			List<PatchElement> validList = new List<PatchElement>();
			for (int i = downloadList.Count - 1; i >= 0; i--)
			{
				var element = downloadList[i];
				if (CheckContentIntegrity(element))
				{
					validList.Add(element);
					downloadList.RemoveAt(i);
				}
			}

			// 缓存已经下载的有效文件
			if (validList.Count > 0)
				_cache.CacheDownloadPatchFiles(validList);

			return downloadList;
		}

		/// <summary>
		/// 获取DLC的下载列表
		/// </summary>
		public List<PatchElement> GetDLCDownloadList(List<string> dlcLabels)
		{
			List<PatchElement> downloadList = new List<PatchElement>(1000);

			// 准备下载列表
			foreach (var element in _localPatchManifest.ElementList)
			{
				if (element.IsDLC() == false)
					continue;

				// 标签比较
				bool hasLabel = false;
				for (int i = 0; i < dlcLabels.Count; i++)
				{
					if (element.HasDLCLabel(dlcLabels[i]))
					{
						hasLabel = true;
						break;
					}
				}
				if (hasLabel == false)
					continue;

				// 缓存资源比较
				if (_cache.Contains(element.MD5))
					continue;

				downloadList.Add(element);
			}

			// 检测文件是否已经下载完毕
			// 注意：如果玩家在加载过程中强制退出，下次再进入的时候跳过已经加载的文件
			List<PatchElement> validList = new List<PatchElement>();
			for (int i = downloadList.Count - 1; i >= 0; i--)
			{
				var element = downloadList[i];
				if (CheckContentIntegrity(element))
				{
					validList.Add(element);
					downloadList.RemoveAt(i);
				}
			}

			// 缓存已经下载的有效文件
			if (validList.Count > 0)
				_cache.CacheDownloadPatchFiles(validList);

			return downloadList;
		}

		/// <summary>
		/// 检测下载内容的完整性
		/// </summary>
		public bool CheckContentIntegrity(PatchElement element)
		{
			return CheckContentIntegrity(element.MD5, element.SizeBytes);
		}
		public bool CheckContentIntegrity(string manifestPath)
		{
			if(_localPatchManifest.Elements.TryGetValue(manifestPath, out PatchElement element))
			{
				return CheckContentIntegrity(element.MD5, element.SizeBytes);
			}
			else
			{
				MotionLog.Warning($"Not found check content file in patch manifest : {manifestPath}");
				return false;
			}
		}
		private bool CheckContentIntegrity(string md5, long size)
		{
			string filePath = PatchHelper.MakeSandboxCacheFilePath(md5);
			if (File.Exists(filePath) == false)
				return false;

			// 校验沙盒里的补丁文件
			if (_checkLevel == ECheckLevel.CheckSize)
			{
				long fileSize = FileUtility.GetFileSize(filePath);
				if (fileSize == size)
					return true;
			}
			else if (_checkLevel == ECheckLevel.CheckMD5)
			{
				string fileHash = HashUtility.FileMD5(filePath);
				if (fileHash == md5)
					return true;
			}
			else
			{
				throw new NotImplementedException(_checkLevel.ToString());
			}
			return false;
		}

		/// <summary>
		/// 缓存下载的补丁文件
		/// </summary>
		public void CacheDownloadPatchFiles(List<PatchElement> downloadList)
		{
			_cache.CacheDownloadPatchFiles(downloadList);
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
					if (_procedure.Current == EPatchStates.GetWebPatchManifest.ToString())
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

		#region 流程节点回调方法
		/// <summary>
		/// 当获取到WEB服务器的反馈信息
		/// </summary>
		public void OnGetWebResponseData(string data)
		{
			if (string.IsNullOrEmpty(data))
				throw new Exception("Web server response data is null or empty.");

			WebResponse response = JsonUtility.FromJson<WebResponse>(data);
			RequestedGameVersion = new Version(response.GameVersion);
			RequestedResourceVersion = response.ResourceVersion;
			ForceInstall = response.ForceInstall;
			AppURL = response.AppURL;
		}

		/// <summary>
		/// 当服务端的补丁清单下载完毕
		/// </summary>
		public void OnDownloadWebPatchManifest(string content)
		{
			if (_remotePatchManifest != null)
				throw new Exception("Should never get here.");
			_remotePatchManifest = PatchManifest.Deserialize(content);
		}

		/// <summary>
		/// 当服务端的补丁文件下载完毕
		/// </summary>
		public void OnDownloadWebPatchFile()
		{
			// 保存补丁清单
			// 注意：这里会覆盖掉沙盒内的补丁清单文件
			_localPatchManifest = _remotePatchManifest;
			string savePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestFileName);
			PatchManifest.Serialize(savePath, _remotePatchManifest);
		}
		#endregion
	}
}