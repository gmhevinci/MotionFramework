### 资源包收集工具 (AssetBundleCollector)

![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/AssetBundleCollector1.png)

**界面说明**  
不同项目对资源的处理及打包规则都有所不同，我们可以通过以下几项设置来定制化自己的打包规则。  
1. Collect : 收集文件夹内资源进行打包（包括所有子文件夹）
2. Ignore : 忽略文件夹内资源不参与打包（包括所有子文件夹）
3. Label By File Path : AssetBundle标签按照文件路径设置
4. Label By Folder Path : AssetBundle标签按照文件所在的文件夹路径设置 

注意：LabelByFolder会将文件夹内所有资源打在一个AssetBundle文件里。

**自定义打包规则**   
如果内置的打包规则已经不能满足需求，那么我们可以轻松实现自定义打包规则
```C#
using MotionFramework.Editor;

public class MyAssetCollector : IAssetCollector
{
	/// <summary>
	/// 获取资源的打包标签
	/// </summary>
	public string GetAssetBundleLabel(string assetPath)
	{
		// Your code in here
		throw new System.NotImplementedException();
	}
}
```

**什么是变体**  
通过构建变体AssetBundle文件，我们可以使用不同变体的AssetBundle并自由切换它们。例如：高清特效纹理，普通特效纹理；中文艺术字，日文艺术字，韩文艺术字。  
在游戏运行时，可以通过MotionFramework提供的接口设置要使用的变体类型，开发者不用关心这些变体资源的引用关系。

**构建变体的注意事项**  
1. 资源的文件名和文件格式必须一致，否则会造成构建包内该资源的唯一ID不一致，资源关联会失败。

**变体相关的进阶教程**  
请参考[Project-Patch](https://github.com/gmhevinci/Projects/tree/master/Project-Patch)工程