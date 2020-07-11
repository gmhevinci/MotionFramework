### 补间管理器 (TweenManager)

一个轻量化的补间系统：扩展方便，使用灵活，功能强大。

创建补间管理器
```C#
public void Start()
{
  // 创建模块
  MotionEngine.CreateModule<TweenManager>();
}
```

窗口打开动画
```C#
public void PlayWindowOpenAnim()
{
  // 同时并行执行所有节点
  var rootNode = ParallelNode.Allocate(
    _animTrans.TweenScaleTo(0.8f, Vector3.one).SetEase(TweenEase.Bounce.EaseOut), //窗口放大
    _animTrans.TweenAnglesTo(0.4f, new Vector3(0, 0, 720)) //窗口旋转
  );
  TweenManager.Instance.AddNode(rootNode);
}
```

窗口关闭动画
```C#
public void PlayWindowCloseAnim()
{
  // 按顺序执行所有节点
  var rootNode = SequenceNode.Allocate(
    TimerNode.AllocateDelay(1f), //等待一秒
    _animTrans.TweenScaleTo(0.5f, Vector3.zero).SetEase(TweenEase.Bounce.EaseOut), //窗口缩小
    ExecuteNode.Allocate(() => { UITools.CloseWindow<MyWindow>(); }) //关闭窗口
  );
  TweenManager.Instance.AddNode(rootNode);
}
```

默认的公共补间方法一共有30种，还可以使用AnimationCurve补充效果
```C#
public class Test
{
  public AnimationCurve EaseCurve;

  public void PlayAnim()
  {
    var rootNode = this.transform.TweenScaleTo(1f, Vector3.zero).SetEase(EaseCurve);
    TweenManager.Instance.AddNode(rootNode);
  }
}
```

扩展支持任意对象
```C#
public static class UnityEngine_UI_Image_Tween_Extension
{
  public static ColorTween TweenColor(this Image obj, float duration, Color from, Color to)
  {
    ColorTween node = ColorTween.Allocate(duration, from, to);
    node.SetUpdate((result) => { obj.color = result; });
    return node;
  }
  public static ColorTween TweenColorTo(this Image obj, float duration, Color to)
  {
    return TweenColor(obj, duration, obj.color, to);
  }
  public static ColorTween TweenColorFrom(this Image obj, float duration, Color from)
  {
    return TweenColor(obj, duration, from, obj.color);
  }
}
```

1. [Module.Tween/TweenManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Tween/TweenManager.cs)