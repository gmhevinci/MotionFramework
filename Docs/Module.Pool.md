### 游戏对象池管理器 (GameObjectPoolManager)

创建对象池管理器
```C#
public void Start()
{
	// 创建模块
	MotionEngine.CreateModule<GameObjectPoolManager>();
}
```

对象池使用异步使用范例
```C#
using MotionFramework.Pool;

public class Test
{
	private string _npcName = "Model/Npc001";
	private GameObject _npc;

	public void Awake()
	{
		// 创建NPC的对象池
		int capacity = 0;
		GameObjectPoolManager.Instance.CreatePool(_npcName, capacity);
	}
	public void Start()
	{
		// 异步方法
		GameObjectPoolManager.Instance.Spawn(_npcName, SpawnCallback);
	}
	private void SpawnCallback(GameObject go)
	{
		_npc = go;
	}
}
```

对象池使用同步使用范例
```C#
using MotionFramework.Pool;

public class Test
{
	private string _npcName = "Model/Npc001";
	private GameObject _npc;
	private bool _isSpawn = false;

	public void Awake()
	{
		// 创建NPC的对象池
		int capacity = 0;
		GameObjectPoolManager.Instance.CreatePool(_npcName, capacity);
	}
	public void Update()
	{
		if(_isSpawn == false && GameObjectPoolManager.Instance.IsAllPrepare())
		{
			_isSpawn = true;
			_npc = GameObjectPoolManager.Instance.Spawn(_npcName);
		}
	}
}
```

更详细的教程请参考示例代码
1. [Module.Pool/GameObjectPoolManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Pool/GameObjectPoolManager.cs)