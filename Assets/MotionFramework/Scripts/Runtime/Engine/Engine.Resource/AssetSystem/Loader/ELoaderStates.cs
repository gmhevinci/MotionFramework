//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Resource
{
	/// <summary>
	/// 文件加载器状态
	/// </summary>
	internal enum ELoaderStates
	{
		None = 0,
		Download,
		CheckDownload,
		LoadFile,
		CheckFile,
		Success,
		Fail,
	}
}