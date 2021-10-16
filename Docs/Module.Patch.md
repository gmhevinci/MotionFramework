### 补丁管理器 (PatchManager)

创建补丁管理器  
```C#
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
	public bool FoundNewApp; //是否发现了新的安装包
	public bool ForceInstall; //是否需要强制用户安装
	public string AppURL; //App安装的地址
#pragma warning restore 0649
}

private class MyGameVersionParser : IGameVersionParser
{
	public Version GameVersion { private set; get; }
	public int ResourceVersion { private set; get; }
	public bool FoundNewApp { private set; get; }
	public bool ForceInstall { private set; get; }
	public string AppURL { private set; get; }

	bool IGameVersionParser.ParseContent(string content)
	{
		try
		{
			WebResponse response = JsonUtility.FromJson<WebResponse>(content);
			GameVersion = new Version(response.GameVersion);
			ResourceVersion = response.ResourceVersion;
			FoundNewApp = response.FoundNewApp;
			ForceInstall = response.ForceInstall;
			AppURL = response.AppURL;
			return true;
		}
		catch(Exception)
		{
			Debug.LogError($"Parse web response failed : {content}");
			return false;
		}
	}
}

public IEnumerator Start()
{
	// 远程服务器信息
	// 默认配置：在没有配置的平台上会走默认的地址。
	string webServerIP = "http://127.0.0.1";
	string cdnServerIP = "http://127.0.0.1";
	string defaultWebServer = $"{webServerIP}/WEB/PC/GameVersion.php";
	string defaultCDNServer = $"{cdnServerIP}/CDN/PC";
	RemoteServerInfo serverInfo = new RemoteServerInfo(null, defaultWebServer, defaultCDNServer, defaultCDNServer);
	serverInfo.AddServerInfo(RuntimePlatform.Android, $"{webServerIP}/WEB/Android/GameVersion.php", $"{cdnServerIP}/CDN/Android", $"{cdnServerIP}/CDN/Android");
	serverInfo.AddServerInfo(RuntimePlatform.IPhonePlayer, $"{webServerIP}/WEB/Iphone/GameVersion.php", $"{cdnServerIP}/CDN/Iphone", $"{cdnServerIP}/CDN/Iphone");

	// 向WEB服务器投递的数据
	WebPost post = new WebPost
	{
		AppVersion = Application.version, //应用程序版本
		ServerID = PlayerPrefs.GetInt("SERVER_ID_KEY", 0), //最近登录的服务器ID
		ChannelID = 0, //渠道ID
		DeviceUID = string.Empty, //设备唯一ID
		TestFlag = PlayerPrefs.GetInt("TEST_FLAG_KEY", 0) //测试包标记
	};

	// 设置参数
	var createParam = new PatchManager.CreateParameters();
	createParam.IgnoreResourceVersion = false;
	createParam.ClearCacheWhenDirty = false;
	createParam.GameVersionParser = new MyGameVersionParser();
	createParam.WebPoseContent = JsonUtility.ToJson(post); 
	createParam.VerifyLevel = EVerifyLevel.CRC;
	createParam.ServerInfo = serverInfo;
	createParam.AutoDownloadDLC = new string[] { "level1" };
	createParam.AutoDownloadBuildinDLC = true;
	createParam.MaxNumberOnLoad = 4;
	
	// 创建模块
	var patchManager = MotionEngine.CreateModule<PatchManager>(createParam);
	yield return patchManager.InitializeAync();
	
	...

	// 开启补丁更新流程
	PatchManager.Instance.Download();
}
```

更详细的教程请参考示例代码
1. [Module.Patch/PatchManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Patch/PatchManager.cs)