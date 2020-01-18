//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Event;

namespace MotionFramework.Patch
{
	public class PatchEventMessageDefine
	{
		/// <summary>
		/// 操作事件
		/// </summary>
		public class OperationEvent : IEventMessage
		{
			/// <summary>
			/// 操作方式
			/// </summary>
			public EPatchOperation operation;
		}

		/// <summary>
		/// 补丁流程状态改变
		/// </summary>
		public class PatchStatesChange : IEventMessage
		{
			public EPatchStates CurrentStates;
		}

		/// <summary>
		/// 发现强更安装包
		/// </summary>
		public class FoundForceInstallAPP : IEventMessage
		{
			public string NewVersion;
			public string InstallURL;
		}

		/// <summary>
		/// 发现更新文件
		/// </summary>
		public class FoundUpdateFiles : IEventMessage
		{
			public int TotalCount;
			public long TotalSizeKB;
		}

		/// <summary>
		/// 下载文件列表进度
		/// </summary>
		public class DownloadFilesProgress : IEventMessage
		{
			public int TotalDownloadCount;
			public int CurrentDownloadCount;	
			public long TotalDownloadSizeKB;
			public long CurrentDownloadSizeKB;
		}

		/// <summary>
		/// 游戏版本号请求失败
		/// </summary>
		public class GameVersionRequestFailed : IEventMessage
		{
		}

		/// <summary>
		/// 网络上补丁清单下载失败
		/// </summary>
		public class WebPatchManifestDownloadFailed : IEventMessage
		{
		}

		/// <summary>
		/// 网络文件下载失败
		/// </summary>
		public class WebFileDownloadFailed : IEventMessage
		{
			public string URL;
			public string Name;
		}

		/// <summary>
		/// 文件MD5验证失败
		/// </summary>
		public class WebFileMD5VerifyFailed : IEventMessage
		{
			public string Name;
		}
	}
}