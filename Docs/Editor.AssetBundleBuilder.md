### 资源包构建工具 (AssetBundleBuilder)

![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/AssetBundleBuilder1.png)

**界面说明**  
```
Build Version : 补丁包的版本号（资源版本号）
Build Output : 打包完成后的输出路径（在工程目录下）。该路径无法修改！
Force Rebuild : 强制重建会删除当前平台下所有的补丁包文件

Compression : Assetbundle的压缩格式
Append Hash : 生成的AssetBundle文件名称添加Hash信息
Disable Write Type Tree : 禁止写入TypeTree，建议不勾选
Ignore Type Tree Chanages : 忽略TypeTree变化，建议勾选
```

**加密方式**  
要实现Bundle文件加密，只需要实现下面代码
```C#
// 注意：这是一个静态类，类名和方法名固定不能改变。
public static class AssetEncrypter
{
	private const string StrEncryptFolderName = "/Assembly/";

	/// <summary>
	/// 检测文件是否需要加密
	/// </summary>
	public static bool Check(string path)
	{
		return path.Contains(StrEncryptFolderName);
	}

	/// <summary>
	/// 对数据进行加密，并返回加密后的数据
	/// </summary>
	public static byte[] Encrypt(byte[] data)
	{
		// 这里使用你的加密算法
		return data;
	}
}
```

**生成结果**  
生成成功后会在输出目录下找到新生成的补丁文件夹。  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/AssetBundleBuilder2.png)

**补丁清单**  
每次打包都会生成一个名为PatchManifest.bytes的补丁清单，补丁清单内包含了所有资源的信息，例如：名称，版本，大小，MD5

**Jenkins支持**  
```
安卓手机打包调用静态方法：BuildAssetBundle.BuildAndroid
苹果手机打包调用静态方法：BuildAssetBundle.BuildIOS

命令行参数
"forceBuild=false"
"buildVersion=100"
```