### 资源包构建工具 (AssetBundleBuilder)

![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/AssetBundleBuilder1.png)

**界面说明**  
```
Build Output : 打包完成后的输出路径（在工程目录下）。该路径无法修改！
Build Version : 补丁包的版本号（资源版本号）
Compression : Assetbundle的压缩格式
Force Rebuild : 强制重建会删除当前平台下所有的补丁包文件
```

**加密方式**  
要实现AssetBundle文件加密，只需要实现下面代码
```C#
public class AssetEncrypter : IAssetEncrypter
{
	/// <summary>
	/// 检测文件是否需要加密
	/// </summary>
	bool IAssetEncrypter.Check(string filePath)
	{
		// 注意：我们对Entity文件夹内的资源进行了加密
		return filePath.Contains("/entity/");
	}

	/// <summary>
	/// 对数据进行加密，并返回加密后的数据
	/// </summary>
	byte[] IAssetEncrypter.Encrypt(byte[] fileData)
	{
		int offset = 32;
		var temper = new byte[fileData.Length + offset];
		Buffer.BlockCopy(fileData, 0, temper, offset, fileData.Length);
		return temper;
	}
}
```

**生成结果**  
构建成功后会在输出目录下找到补丁文件夹（文件夹名称为本次构建时指定的资源版本号）。  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/AssetBundleBuilder2.png)   
补丁文件夹内包含补丁清单和补丁文件。   
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/AssetBundleBuilder3.png)   

**补丁清单**  
每次打包都会生成一个名为PatchManifest.bytes的补丁清单，补丁清单内包含了所有资源的信息，例如：名称，版本，大小，CRC

**Jenkins支持**  
```C#
// 内部构建方法
private static void BuildInternal(BuildTarget buildTarget)
{
	Debug.Log($"[Build] 开始构建补丁包 : {buildTarget}");

	// 打印命令行参数
	int buildVersion = GetBuildVersion();
	bool isForceBuild = IsForceBuild();
	Debug.Log($"[Build] Version : {buildVersion}");
	Debug.Log($"[Build] 强制重建 : {isForceBuild}");

	// 构建参数
	string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
	AssetBundleBuilder.BuildParameters buildParameters = new AssetBundleBuilder.BuildParameters();
	buildParameters.IsVerifyBuildingResult = true;
	buildParameters.OutputRoot = defaultOutputRoot;
	buildParameters.BuildTarget = buildTarget;
	buildParameters.BuildVersion = buildVersion;
	buildParameters.CompressOption = ECompressOption.LZ4;
	buildParameters.IsForceRebuild = isForceBuild;
	buildParameters.BuildinTags = "base";

	// 执行构建
	AssetBundleBuilder builder = new AssetBundleBuilder();
	builder.Run(buildParameters);

	// 构建完成
	Debug.Log("[Build] 构建完成");
}

// 从构建命令里获取参数
private static int GetBuildVersion()
{
	foreach (string arg in System.Environment.GetCommandLineArgs())
	{
		if (arg.StartsWith("buildVersion"))
			return int.Parse(arg.Split("="[0])[1]);
	}
	return -1;
}
private static bool IsForceBuild()
{
	foreach (string arg in System.Environment.GetCommandLineArgs())
	{
		if (arg.StartsWith("forceBuild"))
			return arg.Split("="[0])[1] == "true" ? true : false;
	}
	return false;
}
```