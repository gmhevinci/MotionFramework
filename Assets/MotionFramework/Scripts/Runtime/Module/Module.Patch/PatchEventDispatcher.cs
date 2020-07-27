//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Event;

namespace MotionFramework.Patch
{
	internal static class PatchEventDispatcher
	{
		public static void SendPatchStepsChangeMsg(EPatchStates currentStates)
		{
			PatchEventMessageDefine.PatchStatesChange msg = new PatchEventMessageDefine.PatchStatesChange();
			msg.CurrentStates = currentStates;
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendFoundForceInstallAPPMsg(string newVersion, string installURL)
		{
			PatchEventMessageDefine.FoundForceInstallAPP msg = new PatchEventMessageDefine.FoundForceInstallAPP();
			msg.NewVersion = newVersion;
			msg.InstallURL = installURL;
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendFoundUpdateFilesMsg(int totalCount, long totalSizeBytes)
		{
			PatchEventMessageDefine.FoundUpdateFiles msg = new PatchEventMessageDefine.FoundUpdateFiles();
			msg.TotalCount = totalCount;
			msg.TotalSizeBytes = totalSizeBytes;
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendDownloadFilesProgressMsg(int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes)
		{
			PatchEventMessageDefine.DownloadFilesProgress msg = new PatchEventMessageDefine.DownloadFilesProgress();
			msg.TotalDownloadCount = totalDownloadCount;
			msg.CurrentDownloadCount = currentDownloadCount;
			msg.TotalDownloadSizeBytes = totalDownloadSizeBytes;
			msg.CurrentDownloadSizeBytes = currentDownloadSizeBytes;
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendGameVersionRequestFailedMsg()
		{
			PatchEventMessageDefine.GameVersionRequestFailed msg = new PatchEventMessageDefine.GameVersionRequestFailed();
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendWebPatchManifestDownloadFailedMsg()
		{
			PatchEventMessageDefine.WebPatchManifestDownloadFailed msg = new PatchEventMessageDefine.WebPatchManifestDownloadFailed();
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendWebFileDownloadFailedMsg(string url, string name)
		{
			PatchEventMessageDefine.WebFileDownloadFailed msg = new PatchEventMessageDefine.WebFileDownloadFailed();
			msg.URL = url;
			msg.Name = name;
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendWebFileCheckFailedMsg(string name)
		{
			PatchEventMessageDefine.WebFileCheckFailed msg = new PatchEventMessageDefine.WebFileCheckFailed();
			msg.Name = name;
			EventManager.Instance.SendMessage(msg);
		}
	}
}