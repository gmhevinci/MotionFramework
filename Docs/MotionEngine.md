### 引擎 (MotionEngine)  

在游戏开发过程中，开发者常常需要定义自己的游戏模块。  

自定义游戏模块
```C#
using MotionFramework;

public class BattleManager : ModuleSingleton<BattleManager>, IModule
{
  void IModule.OnCreate(System.Object param)
  {
    //当模块被创建的时候
  }
  void IModule.OnUpdate()
  {
    //轮询模块
  }
  void IModule.OnGUI()
  {
    //GUI绘制
    //可以显示模块的一些关键信息
  }

  public void Print()
  {
    Debug.Log("Hello world");
  }
}
```

创建游戏模块
```C#
public void Start()
{
  // 创建模块
  MotionEngine.CreateModule<BattleManager>();

  // 带优先级的创建方式
  // 说明：运行时的优先级，优先级越大越早轮询。如果没有设置优先级，那么会按照添加顺序执行
  int priority = 1000;
  MotionEngine.CreateModule<BattleManager>(priority);
}
```

使用游戏模块
```C#
public void Start()
{
  // 通过获取实例调用模块方法
  MotionEngine.GetModule<BattleManager>().Print();

  // 通过全局实例调用模块方法
  BattleManager.Instance.Print();
}
```