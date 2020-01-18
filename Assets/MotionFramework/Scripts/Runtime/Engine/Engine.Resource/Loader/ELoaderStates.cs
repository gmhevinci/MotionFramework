//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Resource
{
	/// <summary>
	/// 文件加载状态
	/// </summary>
	internal enum ELoaderStates
	{
		None = 0,
		LoadDepends,
		CheckDepends,
		LoadFile,
		CheckFile,
		Success,
		Fail,
	}
}