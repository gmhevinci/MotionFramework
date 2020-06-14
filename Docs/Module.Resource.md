### 资源管理器 (ResourceManager)

创建资源管理器
```C#
//需要热更资源（网络游戏为主）
public IEnumerator Start()
{
	// 直接使用通用的补丁管理器
	IBundleServices bundleServices = MotionEngine.GetMoudle<PatchManager>();

	// 设置参数
	var createParam = new ResourceManager.CreateParameters();
	createParam.AssetRootPath = "Assets/GameRes";
	createParam.SimulationOnEditor = true;
	createParam.BundleServices = bundleServices;
	createParam.DecryptServices = null;
	createParam.AutoReleaseInterval = 10;

	// 创建模块
	MotionEngine.CreateModule<ResourceManager>(createParam);
}

//不需要热更资源（单机游戏为主）
public IEnumerator Start()
{
	// 直接使用本地资源服务接口
	LocalBundleServices localBundleServices = new LocalBundleServices();
	yield return localBundleServices.InitializeAsync();

	// 设置参数
	var createParam = new ResourceManager.CreateParameters();
	createParam.AssetRootPath = "Assets/GameRes";
	createParam.SimulationOnEditor = true;
	createParam.BundleServices = localBundleServices;
	createParam.DecryptServices = null;
	createParam.AutoReleaseInterval = 10;

	// 创建模块
	MotionEngine.CreateModule<ResourceManager>(createParam);
}
```

资源的加载教程请参考[资源系统](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Resource.md)

更详细的教程请参考示例代码
1. [Module.Resource/ResourceManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Resource/ResourceManager.cs)
