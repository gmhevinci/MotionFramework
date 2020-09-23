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

链式编程
```C#
public void PlayWindowOpenAnim()
{
  ITweenChain tween = SequenceNode.Allocate();
  tween.Delay(1f);
  tween.Append(this.transform.TweenScaleTo(0.5f, Vector3.zero).SetEase(TweenEase.Bounce.EaseOut));
  tween.Execute(() => { Debug.Log("Hello"); });
  TweenManager.Instance.Play(tween);
}
```

默认的公共补间方法一共有30种，还可以使用AnimationCurve补充效果
```C#
public class Test
{
  public AnimationCurve EaseCurve;

  public void PlayAnim()
  {
    var tween = this.transform.TweenScaleTo(1f, Vector3.zero).SetEase(EaseCurve);
    TweenManager.Instance.Play(tween);
  }
}
```

扩展支持任意对象
```C#
// 扩展支持Image对象
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