### 补丁管理器 (PatchManager)

创建补丁管理器  
```C#
public IEnumerator Start()
{
	// 远程服务器信息
	// 默认配置：在没有配置的平台上会走默认的地址。
	string webServer = "http://127.0.0.1";
	string cdnServer = "http://127.0.0.1";
	string defaultWebServerIP = $"{webServer}/WEB/PC/GameVersion.php";
	string defaultCDNServerIP = $"{cdnServer}/CDN/PC";
	RemoteServerInfo serverInfo = new RemoteServerInfo(defaultWebServerIP, defaultCDNServerIP);
	serverInfo.AddServerInfo(RuntimePlatform.Android, $"{webServer}/WEB/Android/GameVersion.php", $"{cdnServer}/CDN/Android");
	serverInfo.AddServerInfo(RuntimePlatform.IPhonePlayer, $"{webServer}/WEB/Iphone/GameVersion.php", $"{cdnServer}/CDN/Iphone");

	// 设置参数
	var createParam = new PatchManager.CreateParameters();
	createParam.ServerID = PlayerPrefs.GetInt("SERVER_ID_KEY", 0); //最近登录的服务器ID
	createParam.ChannelID = 0; //渠道ID
	createParam.DeviceID = 0; //设备唯一ID
	createParam.TestFlag = PlayerPrefs.GetInt("TEST_FLAG_KEY", 0); //测试包标记
	createParam.CheckLevel = ECheckLevel.CheckSize;
	createParam.ServerInfo = serverInfo;
	createParam.VariantRules = null;

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