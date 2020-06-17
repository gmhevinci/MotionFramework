### 配表管理器 (ConfigManager)

[FlashExcel](https://github.com/gmhevinci/FlashExcel)导表工具会自动生成表格相关的CS脚本和二进制数据文件

创建配表管理器
```C#
using MotionFramework.Config;

public void Start()
{
	// 创建模块
	MotionEngine.CreateModule<ConfigManager>(ConfigManager.DefaultParameters);
}
```

加载多个表格的方法
```C#
public class Test
{
	public IEnumerator Start()
	{
		List<string> locations = new List<string>();
		locations.Add("Config/AutoGenerateLanguage");
		locations.Add("Config/ConfigName1");
		locations.Add("Config/ConfigName2");
		locations.Add("Config/ConfigName3");
		yield return ConfigManager.Instance.LoadConfigs(locations);
	}
}
```

加载单个表格的方法
```C#
public class Test
{
	public IEnumerator Start()
	{
		var languageConfig = ConfigManager.Instance.LoadConfig("Config/AutoGenerateLanguage");
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