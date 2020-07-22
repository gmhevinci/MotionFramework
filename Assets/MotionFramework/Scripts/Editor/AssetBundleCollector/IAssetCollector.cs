//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Editor
{
	public interface IAssetCollector
	{
		/// <summary>
		/// 获取资源的打包标签
		/// </summary>
		string GetAssetBundleLabel(string assetPath);
	}
}