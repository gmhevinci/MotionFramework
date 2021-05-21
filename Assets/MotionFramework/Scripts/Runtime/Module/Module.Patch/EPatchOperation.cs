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
		/// <summary>
		/// 开始下载补丁清单
		/// </summary>
		BeginDownloadPatchManifest,

		/// <summary>
		/// 开始获取下载列表
		/// </summary>
		BeginGetDownloadList,

		/// <summary>
		/// 开始下载网络文件
		/// </summary>
		BeginDownloadWebFiles,

		/// <summary>
		/// 尝试再次请求游戏版本
		/// </summary>
		TryRequestGameVersion,

		/// <summary>
		/// 尝试再次下载补丁清单
		/// </summary>
		TryDownloadPatchManifest,

		/// <summary>
		/// 尝试再次下载网络文件
		/// </summary>
		TryDownloadWebFiles,
	}
}