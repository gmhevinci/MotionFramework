//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Patch
{
	/// <summary>
	/// 更新状态
	/// </summary>
	public enum EPatchStates
	{
		/// <summary>
		/// 请求最新的游戏版本
		/// </summary>
		RequestGameVersion,

		/// <summary>
		/// 分析网络上的补丁清单
		/// </summary>
		ParseWebPatchManifest,

		/// <summary>
		/// 获取下载列表
		/// </summary>
		GetDonwloadList,

		/// <summary>
		/// 下载网络文件到沙盒
		/// </summary>
		DownloadWebFiles,

		/// <summary>
		/// 下载结束
		/// </summary>
		DownloadOver,
	}
}