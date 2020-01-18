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
		None,

		#region 初始化阶段
		/// <summary>
		/// 初始化开始
		/// </summary>
		InitiationBegin,

		/// <summary>
		/// 检测沙盒是否变脏
		/// 注意：在覆盖安装的时候，会保留沙盒目录里的文件，所以需要强制清空。
		/// </summary>
		CheckSandboxDirty,

		/// <summary>
		/// 分析APP内的补丁清单
		/// </summary>
		ParseAppPatchManifest,

		/// <summary>
		/// 分析沙盒内的补丁清单
		/// </summary>
		ParseSandboxPatchManifest,

		/// <summary>
		/// 初始化结束
		/// </summary>
		InitiationOver,
		#endregion

		#region 下载阶段
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
		/// 下载网络补丁清单到沙盒
		/// </summary>
		DownloadWebPatchManifest,

		/// <summary>
		/// 下载结束
		/// </summary>
		DownloadOver,
		#endregion
	}
}