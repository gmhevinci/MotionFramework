//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Editor
{
	public interface IAssetRedundancy
	{
		/// <summary>
		/// 检测是否冗余
		/// </summary>
		bool Check(string filePath);
	}
}