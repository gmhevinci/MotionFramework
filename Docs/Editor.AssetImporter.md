### 资源导入工具 (AssetImporter)

![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/AssetImporter1.png)  

在项目开发过程中，有大量相同类型的资源放置在同一个文件夹内，例如：音频，纹理等。每次添加新资源都要手动设置资源的格式，即使能批量修改也是非常麻烦。资源导入工具可以帮助我们简化资源格式设置工作，点击 + 号可以添加要自动处理的资源文件路径，资源导入器代表着一套处理规则，工具提供了一个默认的资源导入器(DefaultProcessor)。

**默认的导入器(DefaultProcessor)**  
同一个文件夹内所有资源的格式以列表里首个资源为准，当导入新资源的时候，会按照首个资源的格式来设置新导入资源的格式。当修改首个资源格式的时候，也会自动修改该文件夹下其它所有资源的格式。

**支持的资源类型**  
目前支持：图片，音频，模型，图集

**自定义导入器**  
要实现自定义导入器，需要继承并实现IAssetProcessor接口。  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/AssetImporter2.png) 

**特殊标记@**  
文件名称末尾带特殊标记的资源文件不会进入自动化流程里。例如：bg_city@