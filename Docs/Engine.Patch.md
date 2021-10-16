### 补丁系统 (Engine.Patch)

**补丁流程**  
1. 请求游戏版本 (RequestGameVersion)
2. 请求远端的补丁清单（RequestPatchManifest）
3. 获取下载列表 (GetDonwloadList)
4. 下载远端文件 (DownloadWebFiles)
6. 下载结束（全部下载成功） (DownloadOver)
7. 补丁流程完毕（PatchDone）

**正常引起的流程挂起**  
1. 当发现新的安装APP的时候，流程系统会被挂起。如果不是强更，那么发送(EPatchOperation.BeginGetDownloadList)事件可以恢复流程系统。
2. 当发现更新文件的时候，流程系统会被挂起。发送(EPatchOperation.BeginDownloadWebFiles)事件可以恢复流程系统。

**异常引起的流程挂起**  
1. 当请求游戏版本号失败的时候，流程系统会被挂起。发送(EPatchOperation.TryRequestGameVersion)事件可以恢复流程系统，然后再次尝试请求游戏版本号。
2. 当请求远端的补丁清单失败的时候，流程系统会被挂起。发送(EPatchOperation.TryRequestPatchManifest)事件可以恢复流程系统，然后再次尝试下载。
3. 当下载网络文件失败的时候，流程系统会被挂起。发送(EPatchOperation.TryDownloadWebFiles)事件可以恢复流程系统，然后再次尝试下载。
4. 当下载的网络文件完整性验证失败的时候，流程系统会被挂起。发送(EPatchOperation.TryDownloadWebFiles)事件可以恢复流程系统，然后再次尝试下载。

**业务逻辑层需要监听的补丁事件**  
```C#
PatchEventMessageDefine.PatchStatesChange //补丁流程状态改变
PatchEventMessageDefine.FoundNewApp //发现了新的安装包
PatchEventMessageDefine.FoundUpdateFiles //发现更新文件
PatchEventMessageDefine.DownloadProgressUpdate //下载进度更新
PatchEventMessageDefine.GameVersionRequestFailed //游戏版本请求失败
PatchEventMessageDefine.GameVersionParseFailed //游戏版本号解析失败
PatchEventMessageDefine.PatchManifestRequestFailed //远端的补丁清单请求失败
PatchEventMessageDefine.WebFileDownloadFailed //网络文件下载失败
PatchEventMessageDefine.WebFileCheckFailed //网络文件验证失败
```

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
	public bool FoundNewApp; //是否发现了新的安装包
	public bool ForceInstall; //是否需要强制用户安装
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
3. 补丁包可以在工程目录下找到，例如：Demo1\Bundles\StandaloneWindows64\

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
		'FoundNewApp'=>false,
		'ForceInstall'=>false,
		'AppURL'=>'www.baidu.com',
	);

	echo json_encode($data);
?>
````