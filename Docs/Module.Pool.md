### 游戏对象池管理器 (GameObjectPoolManager)

创建对象池管理器
```C#
public void Start()
{
	// 创建模块
	GameObjectPoolManager.CreateParameters createParam = new GameObjectPoolManager.CreateParameters();
	createParam.DefaultInitCapacity = 0;
	createParam.DefaultMaxCapacity = 9999;
	createParam.DefaultDestroyTime = 5f;
	MotionEngine.CreateModule<GameObjectPoolManager>(createParam);
}
```

对象池使用范例
```C#
using MotionFramework.Pool;

public class Test
{
	private SpawnGameObject _spawnGo;

	public void Start()
	{
		// 获取对象
		_spawnGo = GameObjectPoolManager.Instance.Spawn("Model/Npc001");
		_spawnGo.Completed += OnAssetLoad;
	}
	public void Destroy()
	{
		// 回收对象
		_spawnGo.Restore();
	}
	private void OnAssetLoad(GameObject go)
	{
	}
}
```

批量创建对象池
```C#
using MotionFramework.Pool;

public class Test
{
	public void Awake()
	{
		GameObjectPoolManager.Instance.CreatePool("Model/Npc001");
		GameObjectPoolManager.Instance.CreatePool("Model/Npc002");
		GameObjectPoolManager.Instance.CreatePool("Model/Npc003");
	}
	public void Update()
	{
		// 等待所有对象池资源加载完毕
		if(GameObjectPoolManager.Instance.IsAllPrepare())
		{
			// Do somthing
		}
	}
}
```

更详细的教程请参考示例代码
1. [Module.Pool/GameObjectPoolManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Pool/GameObjectPoolManager.cs)