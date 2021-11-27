### 网络管理器 (NetworkManager)

创建网络管理器
```C#
public void Start()
{
    // 创建模块
    // 注意：ProtoNetworkPackageCoder是自定义的网络包编码解码器
    var createParam = new NetworkManager.CreateParameters();
    createParam.PackageCoderType = typeof(ProtoNetworkPackageCoder);
    createParam.PackageMaxSize = ushort.MaxValue;
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
        NetworkManager.Instance.NetworkPackageCallback += OnHandleNetworkPackage;
    }

    private void OnHandleEventMessage(IEventMessage eventMessage)
    {
        // 当服务器连接成功
        if(eventMessage is NetworkEventMessageDefine.ConnectSuccess)
        {
            C2R_Login msg = new C2R_Login();
            msg.Account = "test";
            msg.Password = "1234567";
            int msgID = 100;
            
            // 备注：ProtobufHelper接口开发者可以自己实现
            DefaultNetworkPackage package = new DefaultNetworkPackage();
			package.MsgID = msgID;
			package.BodyBytes = ProtobufHelper.Encode(msg);
            NetworkManager.Instance.SendMessage(package);
        }
    }

    private void OnHandleNetworkPackage(INetworkPackage networkPackage)
    {
        // 备注：ProtobufHelper接口开发者可以自己实现
        DefaultNetworkPackage package = networkPackage as DefaultNetworkPackage;
        R2C_Login msg = (R2C_Login)ProtobufHelper.Decode(package.MsgID, package.BodyBytes);
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