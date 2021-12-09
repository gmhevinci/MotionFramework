### 补丁管理器 (PatchManager)

创建补丁管理器  

##### 离线运行模式（适合单机游戏）

```C#
public IEnumerator Initialize()
{
    // 创建补丁管理器
    var createParam = new PatchManager.OfflinePlayModeParameters();
    createParam.SimulationOnEditor = SimulationOnEditor;
    MotionEngine.CreateModule<PatchManager>(createParam);

    // 初始化补丁系统
    var operation = patchManager.InitializeAync();
    yield return operation;
    
    // 开始游戏
    ......
}
```



##### 网络运行模式（适合有资源更新需求的游戏）

```C#
public IEnumerator Initialize()
{
    // 创建补丁管理器
    var createParam = new PatchManager.HostPlayModeParameters();
    createParam.SimulationOnEditor = SimulationOnEditor;
    createParam.ClearCacheWhenDirty = false;
    createParam.IgnoreResourceVersion = false;
    createParam.VerifyLevel = EVerifyLevel.CRC;
    createParam.DefaultHostServer = "http://127.0.0.1/CDN/Android";
    createParam.FallbackHostServer = "http://127.0.0.1/CDN/Android";
    MotionEngine.CreateModule<PatchManager>(createParam);

    // 初始化补丁系统
    var operation = PatchManager.Instance.InitializeAync();
    yield return operation;
}

// 1. 获取资源版本
private int _resourceVersion = 0;
public IEnumerator UpdateResourceVersion()
{
    // 开发者可以通过HTTP向服务器请求最新的资源版本号
    // 备注：如果忽略了资源版本（IgnoreResourceVersion），那么可以跳过这一步    
    ......
}

// 2. 更新资源清单文件
public IEnumerator UpdateManifest()
{
    // 更新资源清单文件
    var operation = PatchManager.Instance.UpdateManifestAsync(_resourceVersion, 30);
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

// 3. 创建DLC下载器
private PatchDownloader _downloader;
public void CreateDownloader()
{
    string[] tags = new string[] {"buildin"};
    int fileLoadingMaxNumber = 10;
    int failedTryAgain = 3;
    _downloader = PatchManager.Instance.CreateDLCDownloader(tags, fileLoadingMaxNumber, failedTryAgain);

    if (_downloader.TotalDownloadCount == 0)
    {
        Debug.Log("没有发现更新文件");
        StartGame();
    }
    else
    {
        int totalDownloadCount = _downloader.TotalDownloadCount;
        long totalDownloadBytes = _downloader.TotalDownloadBytes;
        Debug.Log($"一共有{totalDownloadCount}个文件需要更新，总共文件大小：{totalDownloadBytes}字节");
    }
}

// 4. 下载游戏内容(DLC)
public IEnumerator DownloadFiles()
{
    // 注册相关委托
    _downloader.OnPatchFileCheckFailedCallback = OnWebFileCheckFailed;
    _downloader.OnPatchFileDownloadFailedCallback = OnWebFileDownloadFailed;
    _downloader.OnDownloadProgressCallback = OnDownloadProgressUpdate;
    _downloader.Download();
    yield return _downloader;

    // 检测下载结果
    if (_downloader.DownloadStates != EDownloaderStates.Succeed)
    {
        // 如果中途下载失败，那么可以返回步骤3重新下载
        Debug.Log("游戏内容更新失败")
    }
    else
    {
        Debug.Log("游戏内容更新成功")
        StartGame();
    }
}
public void UpdateDownloader()
{
    // 注意：下载器需要开发者自己维护更新
    if(_downloader != null)
        _downloader.Update();
}
public void DestroyDownloader()
{
    if(_downloader != null)
    {
        _downloader.Forbid();
        _downloader = null;
    }
}
```



更详细的教程请参考示例代码

1. [Module.Patch/PatchManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Patch/PatchManager.cs)