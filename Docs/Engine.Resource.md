### 资源系统 (Engine.Resource)

**编辑器下的模拟运行方式**  
编辑器下的模拟运行方式，主要是使用UnityEditor.AssetDatabase加载资源，用来模拟和AssetBundle一样的运行效果。

**资源定位的根路径**  
所有通过代码加载的资源文件都需要放在资源定位的根路径下，在加载这些资源的时候只需要提供相对路径，资源系统统一约定该相对路径名称为：**location**   

**AssetBundle服务接口**  
在使用AssetBundle加载模式的时候，我们需要提供实现了IBundleServices接口的对象，这个接口主要是提供了资源间依赖关系的查询工作，以及获取AssetBundle文件的加载路径。

**文件解密服务器接口**  
如果AssetBundle文件被加密，那么我们需要提供实现了IDecryptServices接口的对象。

**资源加载 - 委托方式**  
````C#
// 加载主资源对象，不用指定资源对象名称
private void Start()
{
	AssetReference assetRef = new AssetReference("Audio/bgMusic");
	assetRef.LoadAssetAsync<AudioClip>().Completed += Handle_Completed;
}
private void Handle_Completed(AssetOperationHandle obj)
{
	if(obj.AssetObject == null) return;
	AudioClip audioClip = obj.AssetObject as AudioClip;
}
````

````C#
// 加载资源对象，指定资源对象名称
private void Start()
{
	AssetReference assetRef = new AssetReference("Texture/LoadingTextures");
	assetRef.LoadAssetAsync<Texture>("bg1").Completed += Handle_Completed1;
	assetRef.LoadAssetAsync<Texture>("bg2").Completed += Handle_Completed2;
}
private void Handle_Completed1(AssetOperationHandle obj)
{
	if(obj.AssetObject == null) return;
	Texture tex = obj.AssetObject as Texture;
}
private void Handle_Completed2(AssetOperationHandle obj)
{
	if(obj.AssetObject == null) return;
	Texture tex = obj.AssetObject as Texture;
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

	AssetReference assetRef = new AssetReference("Scene/Town");
	assetRef.LoadAssetAsync<SceneInstance>(param).Completed += Handle_Completed1;
}
private void Handle_Completed(AssetOperationHandle obj)
{
	if(obj.AssetObject == null) return;
	SceneInstance instance = obj.AssetObject as SceneInstance;
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
	AssetReference assetRef = new AssetReference("UITexture/bg1");
	AssetOperationHandle handle = assetRef.LoadAssetAsync<Texture>();
	yield return handle;
	Texture bg = handle.AssetObject as Texture;
	Debug.Log(bg.name);
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
	AssetReference assetRef = new AssetReference("UITexture/bg1");
	AssetOperationHandle handle = assetRef.LoadAssetAsync<Texture>();
	await handle.Task;
	Texture bg = handle.AssetObject as Texture;
	Debug.Log(bg.name);
}
````

**资源卸载**  
````C#
public void Start()
{
	AssetReference assetRef = new AssetReference("UITexture/bg1");
	assetRef.LoadAssetAsync<Texture>();

	...

	// 卸载资源
	assetRef.Release();
}
````