# MotionFramework
MotionFramework是一套基于Unity3D引擎的游戏框架。框架整体遵循**轻量化、易用性、低耦合、扩展性强**的设计理念。工程结构清晰，代码注释详细，该框架已被应用于多款商业化的游戏项目，是作为创业游戏公司、独立游戏开发者、以及初学者们推荐的游戏框架。

![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/framework.png)

## 支持版本
Unity2018.4+

## 开发环境
C# && .NET4.x

## 核心系统

1. [引擎](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/MotionEngine.md) - 游戏框架的核心类，它负责游戏模块的创建和管理。在核心系统的基础上，提供了许多在游戏开发过程中常用的管理器，可以帮助开发者加快游戏开发速度。
2. [日志](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/MotionLog.md) - 游戏框架的日志系统，开发者通过注册可以监听框架生成的日志。
3. [控制台](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Console.md) - 在游戏运行的时候，通过内置的控制台可以方便查看调试信息。控制台预设了游戏模块，游戏日志，应用详情，资源系统，引用池，游戏对象池等窗口。开发者可以扩展自定义窗口。
4. [引用池](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Reference.md) - 用于C#引用类型的对象池，对于频繁创建的引用类型，使用引用池可以帮助减少GC。
5. [资源系统](https://github.com/tuyoogame/YooAsset) - 依赖于经过商业化产品验证的YooAsset资源系统。
6. [网络系统](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Network.md) - 异步IOCP SOCKET长连接方案，支持TCP和UDP协议。支持同时建立多个通信频道，例如连接逻辑服务器的同时还可以连接聊天服务器。不同的通信频道支持使用不同的网络包编码解码器，开发者可以扩展支持ProtoBuf的网络包编码解码器，也可以使用自定义的序列化和反序列化方案。
8. [有限状态机](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.AI.FSM.md) - 流程状态机是一种简化的有限状态机。通过流程状态机可以将复杂的业务逻辑拆分简化，例如：整个资源热更新流程可以拆分成多个独立的步骤。

## 管理器介绍
游戏开发过程中常用的管理器

1. [事件管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Event.md) **(EventManager)**
2. [网络管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Network.md) **(NetworkManager)**
3. [资源管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Resource.md) **(ResourceManager)**
4. [音频管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Audio.md) **(AudioManager)**
5. [配表管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Config.md) **(ConfigManager)**
6. [场景管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Scene.md) **(SceneManager)**
7. [窗口管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Window.md) **(WindowManager)**
8. [补间管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Tween.md) **(TweenManager)**
10. [游戏对象池管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Pool.md) **(GameObjectPoolManager)**

## 新手教程
1. [游戏启动器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/GameLauncher.md) **(GameLauncher)**

## DEMO
[Demo](https://github.com/gmhevinci/Demo) 使用MotionFramework制作的一款RPG单机游戏。


## 代码规范

[请参考代码规范](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/CodeStyle.md)

## 贡献者

[何冠峰](https://github.com/gmhevinci) [任志勇](https://github.com/renruoyu1989) [ZensYue](https://github.com/ZensYue) [徐烜](https://github.com/mayaxu) [张飞涛](https://github.com/zhangfeitao)  
