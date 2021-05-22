//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Patch
{
	/// <summary>
	/// 补丁系统更新状态
	/// </summary>
	public enum EPatchStates
	{
		/// <summary>
		/// 请求游戏版本
		/// </summary>
		RequestGameVersion,

		/// <summary>
		/// 请求远端的补丁清单
		/// </summary>
		RequestPatchManifest,

		/// <summary>
		/// 获取下载列表
		/// </summary>
		GetDonwloadList,
		
		/// <summary>
		/// 下载远端文件
		/// </summary>
		DownloadWebFiles,

		/// <summary>
		/// 下载结束（全部成功下载完毕）
		/// </summary>
		DownloadOver,

		/// <summary>
		/// 补丁流程完毕
		/// </summary>
		PatchDone,
	}
}