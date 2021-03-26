### 窗口管理器 (WindowManager)

框架的窗口系统是基于栈式的UI系统，框架已经默认支持了UGUI，并可以扩展支持NGUI，FairyGUI等UI系统

创建窗口管理器
```C#
public IEnumerator Start()
{
	// 创建模块
	MotionEngine.CreateModule<WindowManager>();

	// 加载UIRoot
	// 注意：CanvasRoot是框架扩展支持UGUI的UIRoot
	yield return WindowManager.Instance.CreateUIRoot<CanvasRoot>("UIPanel/UIRoot");
}
```

定义一个窗口对象
```C#
using UnityEngine;
using UnityEngine.UI;
using MotionFramework.Window;

[Window((int)EWindowLayer.Panel, true)]
sealed class UILogin : CanvasWindow
{
	private UISprite _loginSprite;
	private InputField _account;
	private InputField _password;

	// 窗口创建（窗口生命周期内只被调用一次）
	public override void OnCreate()
	{
		_loginSprite = GetUIComponent<UISprite>("UILogin/Window/Button (Login)");
		_account = GetUIComponent<InputField>("UILogin/Window/Content/Text Field (Username)");
		_password = GetUIComponent<InputField>("UILogin/Window/Content/Text Field (Password)");

		// 监听按钮点击事件
		AddButtonListener("UILogin/Window/Button (Login)", OnClickLogin);
	}

	// 窗口销毁（窗口生命周期内只被调用一次）
	public override void OnDestroy()
	{
	}

	// 窗口刷新（窗口生命周期内可能被调用多次）
	public override void OnRefresh()
	{
	}

	// 窗口更新（窗口生命周期内每帧被调用）
	public override void OnUpdate()
	{
	}

	private void OnClickLogin()
	{
		Debug.Log($"Account : {_account.text}");
		Debug.Log($"Password : {_password.text}");
	}
}
```

窗口逻辑代码
```C#
using MotionFramework.Window;

public class Test
{
	public void Start()
	{
		// 打开窗口
		string location = $"UIPanel/UILogin";
		WindowManager.Instance.OpenWindow<UILogin>(location);

		// 关闭窗口
		WindowManager.Instance.CloseWindow<UILogin>();
	}
}
```

关于UGUI的设置  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/WindowModule2.png)

关于UGUI的支持  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/WindowModule1.png)
```
UIRoot预制体内必须包含UICamera对象和UIDesktop对象
```

更详细的教程请参考示例代码
1. [Module.Window/WindowManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Window/WindowManager.cs)