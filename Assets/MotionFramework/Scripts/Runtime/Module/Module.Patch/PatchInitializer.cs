//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using MotionFramework.Utility;
using MotionFramework.Resource;
using MotionFramework.Network;

namespace MotionFramework.Patch
{
	internal class PatchInitializer
	{
		/// <summary>
		/// 异步初始化
		/// </summary>
		public IEnumerator InitializeAync(PatchManagerImpl patcher)
		{
			// 处理沙盒被污染
			ProcessSandboxDirty();

			// 分析APP内的补丁清单
			{
				string filePath = AssetPathHelper.MakeStreamingLoadPath(PatchDefine.PatchManifestFileName);
				string url = AssetPathHelper.ConvertToWWWPath(filePath);
				WebDataRequest downloader = new WebDataRequest(url);
				downloader.DownLoad();
				yield return downloader;

				if (downloader.HasError())
				{
					downloader.ReportError();
					downloader.Dispose();
					throw new System.Exception($"Fatal error : Failed download file : {url}");
				}

				MotionLog.Log("Parse app patch manifest.");
				string jsonData = downloader.GetText();
				patcher.ParseAppPatchManifest(jsonData);
				downloader.Dispose();
			}

			// 分析沙盒内的补丁清单
			if (PatchHelper.CheckSandboxPatchManifestFileExist())
			{
				MotionLog.Log($"Parse sandbox patch file.");
				string filePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestFileName);
				string jsonData = File.ReadAllText(filePath);				
				patcher.ParseSandboxPatchManifest(jsonData);
			}
			else
			{
				patcher.ParseSandboxPatchManifest(patcher.AppPatchManifest);
			}
		}

		/// <summary>
		/// 处理沙盒被污染
		/// 注意：在覆盖安装的时候，会保留沙盒目录里的文件，所以需要强制清空。
		/// </summary>
		private void ProcessSandboxDirty()
		{
			string appVersion = PatchManager.Instance.GetAPPVersion();
			string filePath = PatchHelper.GetSandboxStaticFilePath();

			// 如果是首次打开，记录APP版本信息
			if (PatchHelper.CheckSandboxStaticFileExist() == false)
			{
				MotionLog.Log($"Create sandbox static file : {filePath}");
				FileUtility.CreateFile(filePath, appVersion);
				return;
			}

			// 每次启动时比对APP版本号是否一致		
			string recordVersion = FileUtility.ReadFile(filePath);

			// 如果记录的版本号不一致		
			if (recordVersion != appVersion)
			{
				MotionLog.Warning($"Sandbox is dirty, Record version is {recordVersion}, APP version is {appVersion}");
				MotionLog.Warning("Clear all sandbox files.");
				PatchHelper.ClearSandbox();

				// 重新写入最新的APP版本信息
				MotionLog.Log($"Recreate sandbox static file : {filePath}");
				FileUtility.CreateFile(filePath, appVersion);
			}
		}
	}
}