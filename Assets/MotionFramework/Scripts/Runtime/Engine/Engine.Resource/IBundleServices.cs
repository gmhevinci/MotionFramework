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
		/// 将资源定位地址转换为清单路径
		/// </summary>
		string ConvertLocationToManifestPath(string location);

		/// <summary>
		/// 获取AssetBundle的加载路径
		/// </summary>
		string GetAssetBundleLoadPath(string manifestPath);

		/// <summary>
		/// 获取AssetBundle的直接依赖列表
		/// </summary>
		string[] GetDirectDependencies(string assetBundleName);

		/// <summary>
		/// 获取AssetBundle的所有依赖列表
		/// </summary>
		string[] GetAllDependencies(string assetBundleName);
	}
}