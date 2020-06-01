//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Text;
using System.IO;

namespace MotionFramework.Utility
{
    public static class FileUtility
    {
		/// <summary>
		/// 读取文件
		/// </summary>
		public static string ReadFile(string filePath)
		{
			if (File.Exists(filePath) == false)
				return string.Empty;
			return File.ReadAllText(filePath, Encoding.UTF8);
		}

		/// <summary>
		/// 创建文件
		/// </summary>
		public static void CreateFile(string filePath, string content)
		{
			// 删除旧文件
			if (File.Exists(filePath))
				File.Delete(filePath);

			// 创建文件夹路径
			CreateFileDirectory(filePath);

			// 创建新文件
			byte[] bytes = Encoding.UTF8.GetBytes(content);
			using (FileStream fs = File.Create(filePath))
			{
				fs.Write(bytes, 0, bytes.Length);
				fs.Flush();
				fs.Close();
			}
		}

		/// <summary>
		/// 创建文件路径
		/// </summary>
		public static void CreateFileDirectory(string filePath)
		{
			// If the destination directory doesn't exist, create it.
			string destDirectory = Path.GetDirectoryName(filePath);
			if (Directory.Exists(destDirectory) == false)
				Directory.CreateDirectory(destDirectory);
		}

		/// <summary>
		/// 获取文件大小（字节数）
		/// </summary>
		public static long GetFileSize(string filePath)
		{
			FileInfo fileInfo = new FileInfo(filePath);
			return fileInfo.Length;
		}
	}
}