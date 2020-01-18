### 引用池 (Engine.Reference)

定义引用类
```C#
using MotionFramework.Reference;

public class ReferClass : IReference
{
	public int Value = 0;

	// 在回收的时候该方法会被执行
	public void OnRelease()
	{
		Value = 0;
	}
}
```

单个回收范例
```C#
public void Start()
{
	// 获取对象方式1
	{
		ReferClass refer = ReferencePool.Spawn(typeof(ReferClass)) as ReferClass;
		ReferencePool.Release(refer)
	}

	// 获取对象方式2
	{
		ReferClass refer = ReferencePool.Spawn<ReferClass>();
		ReferencePool.Release(refer)
	}
}
```

批量回收范例
```C#
public void Start()
{
	// 回收列表集合
	List<ReferClass> referList = new List<ReferClass>();
	ReferencePool.Release<ReferClass>(referList)

	// 回收数组集合
	ReferClass[] referArray = new ReferClass[10];
	ReferencePool.Release<ReferClass>(referArray)
}
```