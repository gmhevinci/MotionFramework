//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
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
		public static void SendFoundNewAppMsg(bool forceInstall, string installURL, string newVersion)
		{
			PatchEventMessageDefine.FoundNewApp msg = new PatchEventMessageDefine.FoundNewApp();
			msg.ForceInstall = forceInstall;	
			msg.InstallURL = installURL;
			msg.NewVersion = newVersion;
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendFoundUpdateFilesMsg(int totalCount, long totalSizeBytes)
		{
			PatchEventMessageDefine.FoundUpdateFiles msg = new PatchEventMessageDefine.FoundUpdateFiles();
			msg.TotalCount = totalCount;
			msg.TotalSizeBytes = totalSizeBytes;
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendDownloadProgressUpdateMsg(int totalDownloadCount, int currentDownloadCount, long totalDownloadSizeBytes, long currentDownloadSizeBytes)
		{
			PatchEventMessageDefine.DownloadProgressUpdate msg = new PatchEventMessageDefine.DownloadProgressUpdate();
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
		public static void SendGameVersionParseFailedMsg()
		{
			PatchEventMessageDefine.GameVersionParseFailed msg = new PatchEventMessageDefine.GameVersionParseFailed();
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendPatchManifestRequestFailedMsg()
		{
			PatchEventMessageDefine.PatchManifestRequestFailed msg = new PatchEventMessageDefine.PatchManifestRequestFailed();
			EventManager.Instance.SendMessage(msg);
		}
		public static void SendWebFileDownloadFailedMsg(string name)
		{
			PatchEventMessageDefine.WebFileDownloadFailed msg = new PatchEventMessageDefine.WebFileDownloadFailed();
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