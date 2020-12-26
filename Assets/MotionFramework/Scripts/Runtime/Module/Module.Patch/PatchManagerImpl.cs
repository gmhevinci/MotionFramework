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
			public string DeviceUID; //设备唯一ID
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

		// 流程状态机
		private readonly ProcedureFsm _procedure = new ProcedureFsm();

		// 参数相关
		private int _serverID;
		private int _channelID;
		private string _deviceUID;
		private int _testFlag;
		private EVerifyLevel _verifyLevel;
		private RemoteServerInfo _serverInfo;
		private string[] _autoDownloadDLC;
		private int _maxNumberOnLoad;
		
		// 强更标记和APP地址
		public bool ForceInstall { private set; get; } = false;
		public string AppURL { private set; get; }

		// 请求的版本号
		public Version RequestedGameVersion { private set; get; }
		public int RequestedResourceVersion { private set; get; }

		// 补丁清单
		private PatchManifest _appPatchManifest;
		private PatchManifest _localPatchManifest;
		private PatchCache _cache;

		// 补丁下载器
		public PatchDownloader InternalDownloader { private set; get; }


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
		/// 本地的资源版本号
		/// </summary>
		public int LocalResourceVersion
		{
			get
			{
				if (_localPatchManifest == null)
					return -1;
				return _localPatchManifest.ResourceVersion;
			}
		}

		
		public void Create(PatchManager.CreateParameters createParam)
		{
			_serverID = createParam.ServerID;
			_channelID = createParam.ChannelID;
			_deviceUID = createParam.DeviceUID;
			_testFlag = createParam.TestFlag;
			_verifyLevel = createParam.VerifyLevel;
			_serverInfo = createParam.ServerInfo;
			_autoDownloadDLC = createParam.AutoDownloadDLC;
			_maxNumberOnLoad = createParam.MaxNumberOnLoad;
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAsync()
		{
			MotionLog.Log($"Beginning to initialize patch manager.");

			// 加载缓存
			_cache = PatchCache.LoadCache();

			// 检测沙盒被污染
			// 注意：在覆盖安装的时候，会保留沙盒目录里的文件，所以需要强制清空
			{
				// 如果是首次打开，记录APP版本号
				if (PatchHelper.CheckSandboxCacheFileExist() == false)
				{
					_cache.CacheAppVersion = Application.version;
					_cache.SaveCache();
				}
				else
				{
					// 每次启动时比对APP版本号是否一致	
					if (_cache.CacheAppVersion != Application.version)
					{
						MotionLog.Warning($"Cache is dirty ! Cache version is {_cache.CacheAppVersion}, APP version is {Application.version}");
						ClearCache();

						// 重新写入最新的APP版本号
						_cache.CacheAppVersion = Application.version;
						_cache.SaveCache();
					}
				}
			}

			// 加载APP内的补丁清单
			MotionLog.Log($"Load app patch manifest.");
			{
				string filePath = AssetPathHelper.MakeStreamingLoadPath(PatchDefine.PatchManifestFileName);
				string url = AssetPathHelper.ConvertToWWWPath(filePath);
				WebGetRequest downloader = new WebGetRequest(url);
				downloader.DownLoad();
				yield return downloader;

				if (downloader.HasError())
				{
					downloader.ReportError();
					downloader.Dispose();
					throw new System.Exception($"Fatal error : Failed download file : {url}");
				}

				// 解析补丁清单
				string jsonData = downloader.GetText();
				_appPatchManifest = PatchManifest.Deserialize(jsonData);
				downloader.Dispose();
			}

			// 加载沙盒内的补丁清单
			MotionLog.Log($"Load sandbox patch manifest.");
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
		/// 开启更新
		/// </summary>
		public void Download()
		{
			MotionLog.Log("Begin to run patch procedure.");

			// 注意：按照先后顺序添加流程节点
			_procedure.AddNode(new FsmRequestGameVersion(this));
			_procedure.AddNode(new FsmGetWebPatchManifest(this));
			_procedure.AddNode(new FsmGetDonwloadList(this));
			_procedure.AddNode(new FsmDownloadWebFiles(this));
			_procedure.AddNode(new FsmDownloadOver(this));
			_procedure.AddNode(new FsmPatchDone());
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

		/// <summary>
		/// 清空缓存并删除所有沙盒文件
		/// </summary>
		public void ClearCache()
		{
			_cache.ClearCache();
		}

		/// <summary>
		/// 获取AssetBundle的加载信息
		/// </summary>
		public AssetBundleInfo GetAssetBundleInfo(string bundleName)
		{
			if (_localPatchManifest.Elements.TryGetValue(bundleName, out PatchElement element))
			{
				// 查询内置资源
				if (_appPatchManifest.Elements.TryGetValue(bundleName, out PatchElement appElement))
				{
					if (appElement.IsDLC() == false && appElement.MD5 == element.MD5)
					{
						string appLoadPath = AssetPathHelper.MakeStreamingLoadPath(appElement.MD5);
						AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, appLoadPath, string.Empty, appElement.Version, appElement.IsEncrypted);
						return bundleInfo;
					}
				}

				// 查询缓存资源
				// 注意：如果沙盒内缓存文件不存在，那么将会从服务器下载
				string sandboxLoadPath = PatchHelper.MakeSandboxCacheFilePath(element.MD5);
				if (_cache.Contains(element.MD5))
				{
					AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, sandboxLoadPath, string.Empty, element.Version, element.IsEncrypted);
					return bundleInfo;
				}
				else
				{
					string remoteURL = GetWebDownloadURL(element.Version.ToString(), element.MD5);
					AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, sandboxLoadPath, remoteURL, element.Version, element.IsEncrypted);
					return bundleInfo;
				}
			}
			else
			{
				MotionLog.Warning($"Not found element in patch manifest : {bundleName}");
				AssetBundleInfo bundleInfo = new AssetBundleInfo(bundleName, string.Empty);
				return bundleInfo;
			}
		}

		/// <summary>
		/// 获取启动游戏时的下载列表
		/// </summary>
		public List<PatchElement> GetAutoPatchDownloadList()
		{
			return GetPatchDownloadList(_autoDownloadDLC);
		}

		/// <summary>
		/// 获取补丁下载列表
		/// </summary>
		public List<PatchElement> GetPatchDownloadList(string[] dlcLabels)
		{
			List<PatchElement> downloadList = new List<PatchElement>(1000);

			// 准备下载列表
			foreach (var element in _localPatchManifest.ElementList)
			{
				// 忽略缓存资源
				if (_cache.Contains(element.MD5))
					continue;

				// 查询DLC资源
				if (element.IsDLC())
				{
					if (dlcLabels == null)
						continue;
					if (element.HasDLCLabel(dlcLabels) == false)
						continue;
				}

				// 忽略内置资源
				if (_appPatchManifest.Elements.TryGetValue(element.BundleName, out PatchElement appElement))
				{
					if (appElement.IsDLC() == false && appElement.MD5 == element.MD5)
						continue;
				}

				downloadList.Add(element);
			}

			return CacheAndFilterDownloadList(downloadList);
		}

		/// <summary>
		/// 创建内置的加载器
		/// </summary>
		public void CreateInternalDownloader(List<PatchElement> downloadList)
		{
			MotionLog.Log("Create internal patch downloader.");
			InternalDownloader = new PatchDownloader(this, downloadList, _maxNumberOnLoad);
		}

		// 检测下载内容的完整性并缓存
		public bool CheckContentIntegrity(string bundleName)
		{
			if (_localPatchManifest.Elements.TryGetValue(bundleName, out PatchElement element))
			{
				return CheckContentIntegrity(element.MD5, element.CRC32, element.SizeBytes);
			}
			else
			{
				MotionLog.Warning($"Not found check content file in local patch manifest : {bundleName}");
				return false;
			}
		}
		public bool CheckContentIntegrity(PatchElement element)
		{
			return CheckContentIntegrity(element.MD5, element.CRC32, element.SizeBytes);
		}
		private bool CheckContentIntegrity(string md5, uint crc32, long size)
		{
			string filePath = PatchHelper.MakeSandboxCacheFilePath(md5);
			if (File.Exists(filePath) == false)
				return false;

			// 校验沙盒里的补丁文件
			if (_verifyLevel == EVerifyLevel.Size)
			{
				long fileSize = FileUtility.GetFileSize(filePath);
				if (fileSize == size)
					return true;
			}
			else if (_verifyLevel == EVerifyLevel.MD5)
			{
				string fileHash = HashUtility.FileMD5(filePath);
				if (fileHash == md5)
					return true;
			}
			else if(_verifyLevel == EVerifyLevel.CRC32)
			{
				uint fileHash = HashUtility.FileCRC32(filePath);
				if (fileHash == crc32)
					return true;
			}
			else
			{
				throw new NotImplementedException(_verifyLevel.ToString());
			}
			return false;
		}

		// 缓存系统相关
		public void CacheDownloadPatchFile(string bundleName)
		{
			if (_localPatchManifest.Elements.TryGetValue(bundleName, out PatchElement element))
			{
				MotionLog.Log($"Cache download file : {element.BundleName} : {element.Version}");
				_cache.CacheDownloadPatchFile(element.MD5);
			}
			else
			{
				MotionLog.Warning($"Not found cache content file in local patch manifest : {bundleName}");
			}
		}
		public void CacheDownloadPatchFiles(List<PatchElement> downloadList)
		{
			List<string> hashList = new List<string>(downloadList.Count);
			foreach(var element in downloadList)
			{
				MotionLog.Log($"Cache download file : {element.BundleName} : {element.Version}");
				hashList.Add(element.MD5);
			}
			_cache.CacheDownloadPatchFiles(hashList);
		}
		private List<PatchElement> CacheAndFilterDownloadList(List<PatchElement> downloadList)
		{
			// 检测文件是否已经下载完毕
			// 注意：如果玩家在加载过程中强制退出，下次再进入的时候跳过已经加载的文件
			List<PatchElement> cacheList = new List<PatchElement>();
			for (int i = downloadList.Count - 1; i >= 0; i--)
			{
				var element = downloadList[i];
				if (CheckContentIntegrity(element))
				{
					cacheList.Add(element);
					downloadList.RemoveAt(i);
				}
			}

			// 缓存已经下载的有效文件
			if (cacheList.Count > 0)
				CacheDownloadPatchFiles(cacheList);

			return downloadList;
		}

		// 补丁清单相关
		public PatchManifest GetPatchManifest()
		{
			return _localPatchManifest;
		}
		public void ParseRemotePatchManifest(string content)
		{
			_localPatchManifest = PatchManifest.Deserialize(content);
		}
		public void SaveRemotePatchManifest()
		{
			// 注意：这里会覆盖掉沙盒内的补丁清单文件
			MotionLog.Log("Save remote patch manifest.");
			string savePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestFileName);
			PatchManifest.Serialize(savePath, _localPatchManifest);
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
				DeviceUID = _deviceUID,
				TestFlag = _testFlag
			};
			return JsonUtility.ToJson(post);
		}
		public void ParseWebResponseData(string data)
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