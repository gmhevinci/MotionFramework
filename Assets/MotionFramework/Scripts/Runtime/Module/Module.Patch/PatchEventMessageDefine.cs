//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Event;

namespace MotionFramework.Patch
{
	public class PatchEventMessageDefine
	{
		/// <summary>
		/// 补丁流程步骤改变
		/// </summary>
		public class PatchStatesChange : IEventMessage
		{
			public EPatchStates CurrentStates;
		}

		/// <summary>
		/// 发现了新的安装包
		/// </summary>
		public class FoundNewApp : IEventMessage
		{
			public bool ForceInstall;
			public string InstallURL;
			public string NewVersion;
		}

		/// <summary>
		/// 发现更新文件
		/// </summary>
		public class FoundUpdateFiles : IEventMessage
		{
			public int TotalCount;
			public long TotalSizeBytes;
		}

		/// <summary>
		/// 下载进度更新
		/// </summary>
		public class DownloadProgressUpdate : IEventMessage
		{
			public int TotalDownloadCount;
			public int CurrentDownloadCount;
			public long TotalDownloadSizeBytes;
			public long CurrentDownloadSizeBytes;
		}

		/// <summary>
		/// 游戏版本号请求失败
		/// </summary>
		public class GameVersionRequestFailed : IEventMessage
		{
		}

		/// <summary>
		/// 游戏版本号解析失败
		/// </summary>
		public class GameVersionParseFailed : IEventMessage
		{
		}

		/// <summary>
		/// 补丁清单请求失败
		/// </summary>
		public class PatchManifestRequestFailed : IEventMessage
		{
		}

		/// <summary>
		/// 网络文件下载失败
		/// </summary>
		public class WebFileDownloadFailed : IEventMessage
		{
			public string Name;
		}

		/// <summary>
		/// 文件验证失败
		/// </summary>
		public class WebFileCheckFailed : IEventMessage
		{
			public string Name;
		}
	}
}