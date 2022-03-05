### 资源管理器 (ResourceManager)

创建资源管理器

**编辑器下模式运行模式**

````c#
IEnumerator Start()
{
    var createParam = new YooAssets.EditorPlayModeParameters();
    createParam.LocationRoot = GameDefine.AssetRootPath;
    MotionEngine.CreateModule<ResourceManager>(createParam);
    
    var operation = ResourceManager.Instance.InitializeAsync();
    yield return operation;
}
````

**离线运行模式（适合不需要资源更新的单机游戏）**

```C#
IEnumerator Start()
{
    var createParam = new YooAssets.OfflinePlayModeParameters();
    createParam.LocationRoot = GameDefine.AssetRootPath;
    MotionEngine.CreateModule<ResourceManager>(createParam);
    
    var operation = ResourceManager.Instance.InitializeAsync();
    yield return operation;
}
```

**联机运行模式（适合需要资源更新的网络游戏）**  

````c#
// 1. 初始化资源系统
IEnumerator Start()
{
    var createParam = new YooAssets.HostPlayModeParameters();
    createParam.LocationRoot = GameDefine.AssetRootPath;
    createParam.ClearCacheWhenDirty = false;
    createParam.IgnoreResourceVersion = false;
    createParam.DefaultHostServer = "http://127.0.0.1/CDN1/Android";
    createParam.FallbackHostServer = "http://127.0.0.1/CDN2/Android";
    MotionEngine.CreateModule<PatchManager>(createParam);

    var operation = ResourceManager.Instance.InitializeAsync();
    yield return operation;
}

// 2. 更新资源清单文件
IEnumerator UpdateManifest()
{
    // 更新资源清单文件
    // 注意：开发者可以通过HTTP向服务器请求最新的资源版本号
    var operation = ResourceManager.Instance.UpdateManifestAsync(resourceVersion, 30);
    yield return operation;
    
    // 验证资源清单更新结果
    if(operation.Status == EOperationStatus.Succeed)
    {
        Debug.Log("资源清单下载成功");
    }
    else
    {
        // 如果更新失败，可以重新尝试下载
        Debug.Log($"资源清单下载失败:{operation.Error}");      
    }
}

// 3. 补丁下载器
IEnumerator CreateDownloader()
{
    string[] tags = new string[] {"buildin"};
    int fileLoadingMaxNumber = 10;
    int failedTryAgain = 3;
    DownloaderOperation downloader = ResourceManager.Instance.CreateDLCDownloader(tags, fileLoadingMaxNumber, failedTryAgain);

    if (downloader.TotalDownloadCount == 0)
    {
        Debug.Log("没有发现更新文件");
        return;
    }
    
    int totalDownloadCount = downloader.TotalDownloadCount;
    long totalDownloadBytes = downloader.TotalDownloadBytes;
    Debug.Log($"一共有{totalDownloadCount}个文件需要更新，总共文件大小：{totalDownloadBytes}字节");
    
    // 注册相关委托
    downloader.OnDownloadFileFailedCallback = OnWebFileDownloadFailed;
    downloader.OnDownloadProgressCallback = OnDownloadProgressUpdate;
    downloader.OnDownloadOverCallback = OnDownloadOver;
    downloader.BeginDownload();
    yield return downloader;

    // 检测下载结果
    if (downloader.States != EOperationStatus.Succeed)
    {
        Debug.Log("游戏内容更新失败")
    }
    else
    {
        Debug.Log("游戏内容更新成功")
    }
}
````

**加载路径的匹配方式**  

````C#
// 不带扩展名的模糊匹配
ResourceManager.Instance.LoadAssetAsync<Texture>("UITexture/Bcakground");

// 带扩展名的精准匹配
ResourceManager.Instance.LoadAssetAsync<Texture>("UITexture/Bcakground.png");
````

**资源加载 **  

````C#
// 协程加载方式
void Start()
{
    MotionEngine.StartCoroutine(AsyncLoad());
}
IEnumerator AsyncLoad()
{
    AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<AudioClip>("Audio/bgMusic");
    yield return handle;
    AudioClip audioClip = handle.AssetObject as AudioClip;
}
````

````C#
// 异步加载方式
async void Start()
{
    await AsyncLoad();
}
async Task AsyncLoad()
{
    AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<AudioClip>("Audio/bgMusic");
    await handle.Task;
    AudioClip audioClip = handle.AssetObject as AudioClip;
}
````

````C#
// 委托加载方式
void Start()
{
    AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<Texture>("UITexture/Bcakground");
    handle.Completed += Handle_Completed;
}
void Handle_Completed(AssetOperationHandle handle)
{
    Texture bgTexture = handle.AssetObject as Texture;
}
````

**资源卸载**  

````C#
void Start()
{
    AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<AudioClip>("Audio/bgMusic");

    ...

    // 卸载资源
    handle.Release();
}
````



更详细的教程请参考示例代码
1. [Module.Resource/ResourceManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Resource/ResourceManager.cs)
