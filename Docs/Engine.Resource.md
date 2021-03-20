### 资源系统 (Engine.Resource)

这是一套类似于Unity自带的Resources系统。

**编辑器下的模拟运行方式**  
编辑器下的模拟运行方式，主要是使用UnityEditor.AssetDatabase加载资源，用来模拟和AssetBundle一样的运行效果。

**资源定位的根路径**  
所有通过代码加载的资源文件都需要放在资源定位的根路径下，在加载这些资源的时候只需要提供相对路径，资源系统统一约定该相对路径名称为：**location**   

**AssetBundle服务接口**  
在使用AssetBundle加载模式的时候，我们需要提供实现了IBundleServices接口的对象，这个接口主要是提供了资源间依赖关系的查询工作，以及获取AssetBundle文件的相关信息。

**文件解密服务器接口**  
如果AssetBundle文件被加密，那么我们需要提供实现了IDecryptServices接口的对象。

**资源加载 - 委托方式**  
````C#
// 加载音频资源
private void Start()
{
	AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<AudioClip>("Audio/bgMusic");
	handle.Completed += Handle_Completed;
}
private void Handle_Completed(AssetOperationHandle handle)
{
	if(handle.AssetObject == null) return;
	AudioClip audioClip = handle.AssetObject as AudioClip;
}
````

````C#
// 加载纹理资源
private void Start()
{
	AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<Texture>("Texture/LoadingTextures/bg");
	handle.Completed += Handle_Completed;
}
private void Handle_Completed(AssetOperationHandle handle)
{
	if(handle.AssetObject == null) return;
	Texture tex = handle.AssetObject as Texture;
}
````

````C#
// 加载精灵图集
private void Start()
{
	AssetOperationHandle handle = ResourceManager.Instance.LoadSubAssetsAsync<Sprite>("UITexture/Login");
	handle.Completed += Handle_Completed;
}
private void Handle_Completed(AssetOperationHandle handle)
{
	if(handle.AllAssets == null) return;
	foreach (var asset in handle.AllAssets)
	{
		Debug.Log(asset.name);
	}
}
````

````C#
// 加载场景
private void Start()
{
	// 场景加载参数
	SceneInstanceParam param = new SceneInstanceParam();
	param.IsAdditive = false;
	param.ActivateOnLoad = true;

	AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<SceneInstance>("Scene/Login", param);
	handle.Completed += Handle_Completed;
}
private void Handle_Completed(AssetOperationHandle handle)
{
	SceneInstance instance = handle.AssetInstance as SceneInstance;
	Debug.Log(instance.Scene.name);
}
````

**资源加载 - 异步方式**  
````C#
// 协程加载方式
public void Start()
{
	 AppEngine.Instance.StartCoroutine(AsyncLoad());
}
private IEnumerator AsyncLoad()
{
	AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<AudioClip>("Audio/bgMusic");
	yield return handle;
	AudioClip audioClip = handle.AssetObject as AudioClip;
}
````

````C#
// 异步加载方式
public async void Start()
{
	await AsyncLoad();
}
private async Task AsyncLoad()
{
	AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<AudioClip>("Audio/bgMusic");
	await handle.Task;
	AudioClip audioClip = handle.AssetObject as AudioClip;
}
````

**资源卸载**  
````C#
public void Start()
{
	AssetOperationHandle handle = ResourceManager.Instance.LoadAssetAsync<Texture>("Audio/bgMusic");

	...

	// 卸载资源
	handle.Release();
}
````