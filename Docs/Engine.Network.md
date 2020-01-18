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
public class ProtoPackageCoder : DefaultPackageCoder
{
	public ProtoPackageCoder()
	{
		// 设置字段类型
		PackageSizeFieldType = EPackageSizeFieldType.UShort;
		MessageIDFieldType = EMessageIDFieldType.UShort;
	}

	// 编码
	protected override byte[] EncodeInternal(object msgObj)
	{
		return ProtobufHelper.Encode(msgObj);
	}

	// 解码
	protected override object DecodeInternal(Type classType, byte[] bodyBytes)
	{
		return ProtobufHelper.Decode(classType, bodyBytes);
	}
}
```

更详细的教程请参考示例代码
1. [Module.Network/Package/DefaultPackageCoder.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Network/Package/DefaultPackageCoder.cs)