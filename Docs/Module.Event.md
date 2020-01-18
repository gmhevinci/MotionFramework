### 事件管理器 (EventManager)

事件系统支持可回收事件类，对于定义了IReference的事件类会在广播结束后回收该事件对象。  

创建事件管理器
```C#
public void Start()
{
  // 创建模块
  MotionEngine.CreateModule<EventManager>();
}
```

定义事件类
```C#
using MotionFramework.Event;
using MotionFramework.Reference;

public class TestEventMsg : IEventMessage, IReference
{
  public string Value;

  // 在回收的时候该方法会被执行
  public void OnRelease()
  {
    Value = null;
  }
}
```

订阅事件
```C#
using UnityEngine;
using MotionFramework.Event;

public class Test
{
  public void Start()
  {
    EventManager.Instance.AddListener<TestEventMsg>(OnHandleEventMsg);
  }

  public void Destroy()
  {
    EventManager.Instance.RemoveListener<TestEventMsg>(OnHandleEventMsg);
  }

  private void OnHandleEventMsg(IEventMessage msg)
  {
    if(msg is TestEventMsg)
    {
      TestEventMsg temp = msg as TestEventMsg;
      Debug.Log(temp.Value);
    }
  }
}
```

发送事件
```C#
using UnityEngine;
using MotionFramework.Event;
using MotionFramework.Reference;

public class Test
{
  public void Start()
  {
    TestEventMsg msg = ReferencePool.Spawn<TestEventMsg>();  
    msg.Value = "hello world";
    EventManager.Instance.SendMessage(msg);
  }
}
```

1. [Module.Event/EventManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Event/EventManager.cs)