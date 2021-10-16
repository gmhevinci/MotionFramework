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

传统编程
```C#
public void PlayWindowOpenAnim()
{
  // 先放大UI元素，然后再缩小的时候向上位移。同时UI元素渐变消失
  var tween = TweenAllocate.Parallel
    (
      TweenAllocate.Sequence
      (
        this.go.transform.TweenScaleTo(0.15f, new Vector3(1.2f, 1.2f, 1f)),
        TweenAllocate.Parallel
        (
           this.go.transform.TweenScaleTo(0.1f, new Vector3(0.8f, 0.8f, 1f)),
           this.go.transform.TweenPositionTo(0.6f, new Vector3(0, 256, 0))
        )
      ),
      TweenAllocate.Sequence
      (
        TweenAllocate.Delay(0.5f),
        text.TweenColorTo(0.25f, new Color(1f, 1f, 1f, 0f))
      )
    );

    // 在补间动画结束后，销毁UI元素
    tween.SetDispose(() => { GameObject.Destroy(this.go); });
    this.go.PlayTween(tween as ITweenNode);
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