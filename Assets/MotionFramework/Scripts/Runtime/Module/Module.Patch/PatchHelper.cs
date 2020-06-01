//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.IO;
using System.Text;
using MotionFramework.Resource;

namespace MotionFramework.Patch
{
	internal static class PatchHelper
	{
		private const string StrStaticFileName = "static.bytes";

		/// <summary>
		/// 输出日志
		/// </summary>
		public static void Log(ELogLevel logType, string log)
		{
			MotionLog.Log(logType, log);
		}

		/// <summary>
		/// 清空沙盒目录
		/// </summary>
		public static void ClearSandbox()
		{
			string directoryPath = AssetPathHelper.MakePersistentLoadPath(string.Empty);
			Directory.Delete(directoryPath, true);
		}

		/// <summary>
		/// 获取沙盒内静态文件的路径
		/// </summary>
		public static string GetSandboxStaticFilePath()
		{
			return AssetPathHelper.MakePersistentLoadPath(StrStaticFileName);
		}

		/// <summary>
		/// 检测沙盒内静态文件是否存在
		/// </summary>
		public static bool CheckSandboxStaticFileExist()
		{
			string filePath = GetSandboxStaticFilePath();
			return File.Exists(filePath);
		}

		/// <summary>
		/// 检测沙盒内补丁清单文件是否存在
		/// </summary>
		public static bool CheckSandboxPatchManifestFileExist()
		{
			string filePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.PatchManifestFileName);
			return File.Exists(filePath);
		}

		/// <summary>
		/// 检测沙盒内Unity清单文件是否存在
		/// </summary>
		public static bool CheckSandboxUnityManifestFileExist()
		{
			string filePath = AssetPathHelper.MakePersistentLoadPath(PatchDefine.UnityManifestFileName);
			return File.Exists(filePath);
		}
	}
}