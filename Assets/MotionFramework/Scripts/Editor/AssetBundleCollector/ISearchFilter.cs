//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 巴黎雨季 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Editor
{
	public interface ISearchFilter
	{
		/// <summary>
		/// 资源过滤
		/// </summary>
		/// <param name="assetPath">资源路径</param>
		/// <returns>如果收集该资源返回TRUE</returns>
		bool FilterAsset(string assetPath);
	}
}