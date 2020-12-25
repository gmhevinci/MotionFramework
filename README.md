# MotionFramework
MotionFramework是一套基于Unity3D引擎的游戏框架。框架整体遵循**轻量化、易用性、低耦合、扩展性强**的设计理念。工程结构清晰，代码注释详细，该框架已被应用于多款商业化的游戏项目，是作为创业游戏公司、独立游戏开发者、以及初学者们推荐的游戏框架。

![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/framework.png)

## 支持版本
Unity2018.4 && Unity2019.4

## 开发环境
C# && .Net4.x

## 核心系统

1. [引擎](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/MotionEngine.md) **(MotionEngine)** - 游戏框架的核心类，它负责游戏模块的创建和管理。在核心系统的基础上，提供了许多在游戏开发过程中常用的管理器，可以帮助开发者加快游戏开发速度。

2. [日志](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/MotionLog.md) **(MotionLog)** - 游戏框架的日志系统，开发者通过注册可以监听框架生成的日志。

3. [控制台](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Console.md) **(Engine.Console)** - 在游戏运行的时候，通过内置的控制台可以方便查看调试信息。控制台预设了游戏模块，游戏日志，应用详情，资源系统，引用池，游戏对象池等窗口。开发者可以扩展自定义窗口。

4. [引用池](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Reference.md) **(Engine.Reference)** - 用于C#引用类型的对象池，对于频繁创建的引用类型，使用引用池可以帮助减少GC。

5. [资源系统](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Resource.md) **(Engine.Resource)** - 基于资源定位的资源系统（LocationBased AssetSystem），支持自定义加密解密，支持AssetBundle变体，支持DLC内容构建，支持边玩边下载。编辑器下的模拟运行方式可以不依赖于AssetBundle文件，省去每次修改资源都要重新构建的时间。业务逻辑支持协程，异步，委托多种异步加载方式。

6. [补丁系统](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Patch.md) **(Engine.Patch)** - 通过补丁系统可以实现资源热更新。支持版本回退，支持区分审核版本，测试版本，线上版本，支持灰度更新。

7. [网络系统](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.Network.md) **(Engine.Network)** - 异步IOCP SOCKET长连接方案，支持TCP和UDP协议。支持同时建立多个通信频道，例如连接逻辑服务器的同时还可以连接聊天服务器。不同的通信频道支持使用不同的网络包编码解码器，开发者可以扩展支持ProtoBuf的网络包编码解码器，也可以使用自定义的序列化和反序列化方案。

8. [有限状态机](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Engine.AI.FSM.md) **(Engine.AI.FSM)** - 流程状态机是一种简化的有限状态机。通过流程状态机可以将复杂的业务逻辑拆分简化，例如：整个资源热更新流程可以拆分成多个独立的步骤。

## 管理器介绍
游戏开发过程中常用的管理器

1. [事件管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Event.md) **(EventManager)**

2. [网络管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Network.md) **(NetworkManager)**

3. [资源管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Resource.md) **(ResourceManager)**

4. [补丁管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Patch.md) **(PatchManager)**

5. [音频管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Audio.md) **(AudioManager)**

6. [配表管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Config.md) **(ConfigManager)**

7. [场景管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Scene.md) **(SceneManager)**

8. [窗口管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Window.md) **(WindowManager)**

9. [补间管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Tween.md) **(TweenManager)**

10. [游戏对象池管理器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Module.Pool.md) **(GameObjectPoolManager)**

## 工具介绍
内置的相关工具介绍

1. [资源包构建工具](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Editor.AssetBundleBuilder.md) **(AssetBundleBuilder)**

2. [资源包收集工具](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Editor.AssetBundleCollector.md) **(AssetBundleCollector)**

3. [资源导入工具](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Editor.AssetImporter.md) **(AssetImporter)**

4. [资源引用搜索工具](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/Editor.AssetSearch.md) **(AssetSearch)**

## 新手教程
1. [游戏启动器](https://github.com/gmhevinci/MotionFramework/blob/master/Docs/GameLauncher.md) **(GameLauncher)**

## DEMO
1. [Demo](https://github.com/gmhevinci/Demo) 使用MotionFramework制作的一款RPG单机游戏。

2. [Projects](https://github.com/gmhevinci/Projects) 基于XLua和ILRuntime热更新技术的工程例子。

## 商业案例
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/icon1.jpg)  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/icon2.jpg)  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/icon3.jpg)  

## 贡献者
[何冠峰](https://github.com/gmhevinci) [任志勇](https://github.com/renruoyu1989) [ZensYue](https://github.com/ZensYue) [徐烜](https://github.com/mayaxu)  

## 声明
我们将会一直维护该框架，提交的Issue会在一天内回复并开始解决，欢迎加入社区QQ群：654986302
