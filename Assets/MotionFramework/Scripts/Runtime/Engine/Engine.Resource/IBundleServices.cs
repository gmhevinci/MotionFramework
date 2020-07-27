//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Resource
{
	public interface IBundleServices
	{
		/// <summary>
		/// 检测下载内容的完整性
		/// </summary>
		bool CheckContentIntegrity(string manifestPath);
		
		/// <summary>
		/// 获取AssetBundle的信息
		/// </summary>
		AssetBundleInfo GetAssetBundleInfo(string manifestPath);

		/// <summary>
		/// 获取AssetBundle的直接依赖列表
		/// </summary>
		string[] GetDirectDependencies(string manifestPath);

		/// <summary>
		/// 获取AssetBundle的所有依赖列表
		/// </summary>
		string[] GetAllDependencies(string manifestPath);
	}
}