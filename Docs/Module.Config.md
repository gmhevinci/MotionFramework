### 配表管理器 (ConfigManager)

[FlashExcel](https://github.com/gmhevinci/FlashExcel)导表工具会自动生成表格相关的CS脚本和二进制数据文件

创建配表管理器
```C#
using MotionFramework.Config;

public void Start()
{
	// 设置参数
	var ceateParam = new ConfigManager.CreateParameters();
	createParam.BaseFolderPath = "Config";

	// 创建模块
	MotionEngine.CreateModule<ConfigManager>(ceateParam);
}
```

加载表格
```C#
public class Test
{
	public void Start()
	{
		// 加载多语言表
		var languageConfig = ConfigManager.Instance.LoadConfig("AutoGenerateLanguage");
		languageConfig.Completed += OnConfigLoad;
	}
	partial void OnConfigLoad(AssetConfig config)
	{
	}
}
```

加载表格
```C#
public class Test
{
	public IEnumerator Start()
	{
		// 加载多语言表
		var languageConfig = ConfigManager.Instance.LoadConfig("AutoGenerateLanguage");
		yield return languageConfig;
	}
}
```

扩展方法方便直接获取数据
```C#
// 这里扩展了获取数据的方法
public partial class CfgAutoGenerateLanguage
{
	public static CfgAutoGenerateLanguageTable GetConfigTable(int key)
	{
		CfgAutoGenerateLanguage config = ConfigManager.Instance.GetConfig<CfgAutoGenerateLanguage>();
		return config.GetTable(key) as CfgAutoGenerateLanguageTable;
	}
}
```

1. [Module.Config/ConfigManager.cs](https://github.com/gmhevinci/MotionFramework/blob/master/Assets/MotionFramework/Scripts/Runtime/Module/Module.Config/ConfigManager.cs)