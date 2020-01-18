### 网络管理器 (NetworkManager)

创建网络管理器
```C#
public void Start()
{
	// 创建模块
	// 注意：ProtoPackageCoder是自定义的网络包编码解码器
	var createParam = new NetworkManager.CreateParameters();
	createParam.PackageCoderType = typeof(ProtoPackageCoder);
	MotionEngine.CreateModule<NetworkManager>(createParam);
}
```

网络管理器使用示例
```C#
using MotionFramework.Network;

public class Test
{
	public void Start()
	{
		EventManager.Instance.AddListener<NetworkEventMessageDefine.BeginConnect>(OnHandleEventMessage);
		EventManager.Instance.AddListener<NetworkEventMessageDefine.ConnectSuccess>(OnHandleEventMessage);
		EventManager.Instance.AddListener<NetworkEventMessageDefine.ConnectFail>(OnHandleEventMessage);
		EventManager.Instance.AddListener<NetworkEventMessageDefine.Disconnect>(OnHandleEventMessage);

		NetworkManager.Instance.ConnectServer("127.0.0.1", 10002);
		NetworkManager.Instance.MonoPackageCallback += OnHandleMonoPackage;
	}

	private void OnHandleEventMessage(IEventMessage msg)
	{
		// 当服务器连接成功
		if(msg is NetworkEventMessageDefine.ConnectSuccess)
		{
			C2R_Login msg = new C2R_Login();
			msg.Account = "test";
			msg.Password = "1234567";
			NetworkManager.Instance.SendMessage(msg);
		}
	}

	private void OnHandleMonoPackage(INetworkPackage package)
	{
		Debug.Log($"Handle net message : {package.MsgID}");
		R2C_Login msg = package.MsgObj as R2C_Login;
		if(msg != null)
		{
			Debug.Log(msg.Address);
			Debug.Log(msg.Key);
		}
	}
}
```

更详细的教程请参考示例代码
1. [Module.Network/NetworkManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Network/NetworkManager.cs)