### 补丁管理器 (PatchManager)

创建补丁管理器  
```C#
public void Start()
{
	// 设置参数
	var createParam = new PatchManager.CreateParameters();
	createParam.ServerID = PlayerPrefs.GetInt("SERVER_ID_KEY", 0); //最近登录的服务器ID
	createParam.ChannelID = 0; //渠道ID
	createParam.DeviceID = 0; //设备唯一ID
	createParam.TestFlag = PlayerPrefs.GetInt("TEST_FLAG_KEY", 0); //测试包标记

	createParam.WebServers = new Dictionary<RuntimePlatform, string>();
	createParam.WebServers.Add(RuntimePlatform.Android, "127.0.0.1/WEB/Android/GameVersion.php");
	createParam.WebServers.Add(RuntimePlatform.IPhonePlayer, "127.0.0.1/WEB/Iphone/GameVersion.php");

	createParam.CDNServers = new Dictionary<RuntimePlatform, string>();
	createParam.CDNServers.Add(RuntimePlatform.Android, "127.0.0.1/CDN/Android");
	createParam.CDNServers.Add(RuntimePlatform.IPhonePlayer, "127.0.0.1/CDN/Iphone");

	// 服务器的默认地址：在运行的平台没有设置服务器地址的时候，会走默认的地址。
	createParam.DefaultWebServerIP = "127.0.0.1/WEB/PC/GameVersion.php";
	createParam.DefaultCDNServerIP = "127.0.0.1/CDN/PC";

	// 注册变体规则
	var variantRule = new PatchManager.CreateParameters.VariantRule();
	variantRule.VariantGroup = new List<string>() { "CN", "EN", "KR" };
	variantRule.TargetVariant = "EN";
	createParam.VariantRules = new List<PatchManager.CreateParameters.VariantRule>() { variantRule };

	// 创建模块
	MotionEngine.CreateModule<PatchManager>(createParam);

	...

	// 开启补丁更新流程
	PatchManager.Instance.Run();
}
```

更详细的教程请参考示例代码
1. [Module.Patch/PatchManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Patch/PatchManager.cs)