### 游戏启动器 (GameLauncher)

对于独立游戏开发者，我们只需要几行简单的代码就可以部署一个游戏所需要的复杂底层框架。
```C#
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 添加游戏开发中常用的游戏模块的命名空间
using MotionFramework;
using MotionFramework.Event;
using MotionFramework.Resource;
using MotionFramework.Config;
using MotionFramework.Audio;
using MotionFramework.Patch;
using MotionFramework.Scene;
using MotionFramework.Pool;
using MotionFramework.Network;

public class GameLauncher : MonoBehaviour
{
	void Awake()
	{
		// 初始化框架
		bool showConsole = Application.isEditor || Debug.isDebugBuild;
		MotionEngine.Initialize(this, showConsole, HandleMotionFrameworkLog);
	}
	void Start()
	{
		// 创建游戏模块
		CreateGameModules();
	}
	void Update()
	{
		// 更新框架
		MotionEngine.Update();
	}
	void OnGUI()
	{
		// 绘制控制台
		MotionEngine.DrawConsole();
	}

	private void CreateGameModules()
	{
		// 创建事件管理器
		MotionEngine.CreateModule<EventManager>();

		// 创建资源管理器
		var resourceCreateParam = new ResourceManager.CreateParameters();
		resourceCreateParam.LocationRoot = "Assets/ResourceRoot";
		resourceCreateParam.SimulationOnEditor = true;
		resourceCreateParam.BundleServices = new LocalBundleServices();
		resourceCreateParam.DecryptServices = null;
		resourceCreateParam.AutoReleaseInterval = 10f;
		MotionEngine.CreateModule<ResourceManager>(resourceCreateParam);

		// 创建音频管理器
		MotionEngine.CreateModule<AudioManager>();

		// 创建场景管理器
		MotionEngine.CreateModule<SceneManager>();

		// 创建对象池管理器
		MotionEngine.CreateModule<GameObjectPoolManager>();

		// 最后创建游戏业务逻辑相关的自定义模块
		MotionEngine.CreateModule<GameManager>();
		GameManager.Instance.StartGame();
	}

	private void HandleMotionFrameworkLog(ELogLevel logLevel, string log)
	{
		if (logLevel == ELogLevel.Log)
		{
			UnityEngine.Debug.Log(log);
		}
		else if (logLevel == ELogLevel.Error)
		{
			UnityEngine.Debug.LogError(log);
		}
		else if (logLevel == ELogLevel.Warning)
		{
			UnityEngine.Debug.LogWarning(log);
		}
		else if (logLevel == ELogLevel.Exception)
		{
			UnityEngine.Debug.LogError(log);
		}
		else
		{
			throw new NotImplementedException($"{logLevel}");
		}
	}
}
```

下面是一个和业务逻辑相关的自定义模块
```C#
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework;

public class GameManager : ModuleSingleton<GameManager>, IModule
{
	void IModule.OnCreate(object createParam)
	{
		// 这里可以完成一些游戏业务逻辑相关的初始化工作
	}
	void IModule.OnUpdate()
	{
		// 这里可以更新我们的游戏业务逻辑
	}
	void IModule.OnGUI()
	{
	}

	// 开始游戏
	public void StartGame()
	{
		...
	}
}
```