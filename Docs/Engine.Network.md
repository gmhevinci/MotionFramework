### 网络系统 (Engine.Network)

定义网络包编码解码器
```C#
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Network;

/// <summary>
/// Protobuf网络包编码解码器
/// </summary>
public class ProtoNetworkPackageCoder : DefaultNetworkPackageCoder
{
	public ProtoNetworkPackageCoder()
	{
		// 设置字段类型
		PackageSizeFieldType = EPackageSizeFieldType.UShort;
		MessageIDFieldType = EMessageIDFieldType.UShort;
	}
}
```

更详细的教程请参考示例代码
1. [Module.Network/Package/DefaultNetworkPackageCoder.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Network/Package/DefaultNetworkPackageCoder.cs)