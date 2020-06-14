//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Patch
{
	/// <summary>
	/// 用户层反馈的操作方式
	/// </summary>
	public enum EPatchOperation
	{
		BeginingDownloadWebFiles,
		TryRequestGameVersion,
		TryDownloadWebPatchManifest,
		TryDownloadWebFiles,
	}
}