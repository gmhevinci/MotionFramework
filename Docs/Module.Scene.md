### 场景管理器 (SceneManager)

创建场景管理器
```C#
public void Start()
{
	// 创建模块
	MotionEngine.CreateModule<SceneManager>();
}
```

```C#
using MotionFramework.Scene;

public class Test
{
	public void Start()
	{
		// 改变主场景
		// 注意：当改变主场景的时候，之前加载的附加场景将会被卸载
		SceneManager.Instance.ChangeMainScene("Scene/Town", true，null);

		// 加载新的附加场景
		SceneManager.Instance.LoadAdditionScene("Scene/Town_sky", true, null);
		SceneManager.Instance.LoadAdditionScene("Scene/Town_river", true, null);

		...

		// 检测场景是否完毕
		bool isDone = SceneManager.Instance.CheckSceneIsDone("Scene/Town")

		// 获取场景加载进度（0-100）
		int progress = SceneManager.Instance.GetSceneLoadProgress("Scene/Town")
	}
}
```

更详细的教程请参考示例代码
1. [Module.Scene/SceneManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Scene/SceneManager.cs)