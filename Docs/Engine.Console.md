### 控制台 (Engine.Console)

控制台窗口截图
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/console.png)  

初始化控制台  
```C#
public class GameLauncher : MonoBehaviour
{
	void Awake()
	{
		// 初始化控制台
		if (Application.isEditor || Debug.isDebugBuild)
			DeveloperConsole.Initialize();
	}

	void OnGUI()
	{
		// 绘制控制台窗口
		if (Application.isEditor || Debug.isDebugBuild)
			DeveloperConsole.DrawGUI();
	}
}
```

自定义窗口  
```C#
using MotionFramework.Console;

[ConsoleAttribute("窗口标题", 201)]
public class CustomDebugWindow : IConsoleWindow
{
	void IConsoleWindow.OnStart()
	{
	}
	void IConsoleWindow.OnGUI()
	{
		ConsoleGUI.Lable("在这里编写GUI代码");
	}
}
```

更详细的教程请参考示例代码
1. [Module.Console](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Console)