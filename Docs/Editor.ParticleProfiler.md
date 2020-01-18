### 特效性能查看工具 (ParticleProfiler) 

通过该工具，我们可以查看特效在运行时的粒子数量，三角形面数，Drawcall这些影响性能的参数。  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/ParticleProfiler1.png)

查看运行时的时间曲线图  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/ParticleProfiler2.png)

查看所有使用的纹理列表  
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/ParticleProfiler3.png)

**指定一个特效分析场景**  
指定一个单独的特效分析场景可以帮助我们更准确的分析数据。在点击测试按钮的时候，特效会在场景里克隆一个对象用于测试，位置在世界坐标系中心点。你需要设置测试场景里的相机位置和角度，以保证测试特效在相机视椎体内。相机里一些默认开启的特性建议关闭，例如：HDR, MSAA。建议相机不使用天空盒，以避免DrawCall不准确。
![image](https://github.com/gmhevinci/MotionFramework/raw/master/Docs/Image/ParticleProfiler4.png)