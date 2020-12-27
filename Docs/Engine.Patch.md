### 补丁系统 (Engine.Patch)

**补丁流程**  
1. 请求最新的游戏版本 (RequestGameVersion)
2. 获取远端的补丁清单（GetWebPatchManifest）
3. 获取下载列表 (GetDonwloadList)
4. 下载网络文件 (DownloadWebFiles)
6. 下载结束 (DownloadOver)
7. 补丁流程完毕（PatchDone）

注意事项：
1. 当发现更新文件的时候，流程系统会被挂起。发送OperationEvent(EPatchOperation.BeginingDownloadWebFiles)事件可以恢复流程系统。
2. 当请求游戏版本号失败的时候，流程系统会被挂起。发送OperationEvent(EPatchOperation.TryRequestGameVersion)事件可以恢复流程系统，然后再次尝试请求游戏版本号。
3. 当下载网络补丁清单失败的时候，流程系统会被挂起。发送OperationEvent(EPatchOperation.TryDownloadWebPatchManifest)事件可以恢复流程系统，然后再次尝试下载。
4. 当下载网络文件失败的时候，流程系统会被挂起。发送OperationEvent(EPatchOperation.TryDownloadWebFiles)事件可以恢复流程系统，然后再次尝试下载。
5. 当下载的网络文件完整性验证失败的时候，流程系统会被挂起。发送OperationEvent(EPatchOperation.TryDownloadWebFiles)事件可以恢复流程系统，然后再次尝试下载。

**补丁事件**  
整个流程抛出的事件
````
PatchEventMessageDefine.PatchStatesChange：补丁流程状态改变
````

下载阶段抛出的事件
````
PatchEventMessageDefine.FoundForceInstallAPP：发现强更安装包
PatchEventMessageDefine.FoundUpdateFiles：发现更新文件
PatchEventMessageDefine.DownloadFilesProgress：下载文件列表进度
PatchEventMessageDefine.GameVersionRequestFailed：游戏版本号请求失败
PatchEventMessageDefine.WebPatchManifestDownloadFailed：网络上补丁清单下载失败
PatchEventMessageDefine.WebFileDownloadFailed：网络文件下载失败
PatchEventMessageDefine.WebFileCheckFailed：文件验证失败
````

**WEB服务器约定**  
Post数据为Json文本
```C#
class WebPost
{
  	public string AppVersion; //应用程序内置版本
  	public int ServerID; //最近登录的服务器ID
	public int ChannelID; //渠道ID
	public string DeviceUID; //设备唯一ID
	public int TestFlag; //测试标记
}
```

Response数据为Json文本
```C#
class WebResponse
{
	public string GameVersion; //当前游戏版本号
	public int ResourceVersion; //当前资源版本
	public bool ForceInstall; //是否需要强制安装
	public string AppURL; //App安装的地址
}
```

审核版本
````
Web服务器可以根据[应用程序内置版本]来判定是否是审核版本。
````

测试版本
````
Web服务器可以根据[测试标记]来判定是否是测试版本。

客户端可以通过聊天窗口输入GM命令来设置[测试标记]，或者通过隐秘窗口来设置[测试标记]。
````

线上版本
````
非审核版本和测试版本统一称为线上版本。
````

线上版本的灰度更新支持
````
Web服务器根据[渠道ID][最近登录的服务器ID]来判断是否需要灰度更新，并返回灰度服务器的游戏版本。

客户端玩家在从普通服务器切换到灰度服务器的时候，提示玩家重启游戏，此刻设置[最近登录的服务器ID]为要进入的灰度服务器的ID
````

**CDN服务器的部署**  
````
1. CDN服务器需要区分Android版本，IOS版本，PC版本。主要是因为部分AssetBundle文件在三个平台构建的文件会有差异。
2. 部署资源服务器非常简单，只需要把对应平台构建的补丁包拷贝到服务器即可，每次出新的补丁包都需要拷贝。
3. 补丁包可以在工程目录下找到，例如：Demo1\BuildBundles\StandaloneWindows64\

注意：当我们中途构建了强更包的时候，就可以把之前的补丁包都删除了。
````

**GameVersion.php**  
Web服务器的PHP范例
````PHP
<?php
	header('Content-Type:application/json; charset=utf-8');

	$data=array(
		'GameVersion'=>'1.0.0.0',
		'ResourceVersion'=>123,
		'ForceInstall'=>false,
		'AppURL'=>'www.baidu.com',
	);

	echo json_encode($data);
?>
````