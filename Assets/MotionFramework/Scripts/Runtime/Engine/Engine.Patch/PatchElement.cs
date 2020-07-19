//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;

namespace MotionFramework.Patch
{
	[Serializable]
	public class PatchElement
	{
		/// <summary>
		/// 文件名称
		/// </summary>
		public string Name;

		/// <summary>
		/// 文件MD5
		/// </summary>
		public string MD5;

		/// <summary>
		/// 文件大小
		/// </summary>
		public long SizeBytes;

		/// <summary>
		/// 文件版本
		/// </summary>
		public int Version;

		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncrypted;

		/// <summary>
		/// 依赖列表
		/// </summary>
		public string[] Dependencies;

		/// <summary>
		/// 后台下载
		/// 注意：标记为后台下载的资源，不会在补丁系统初始化的时候强制下载
		/// </summary>
		public bool BackgroundDownload = false;
		
		public PatchElement(string name, string md5, long sizeBytes, int version, bool isEncrypted, string[] dependencies)
		{
			Name = name;
			MD5 = md5;
			SizeBytes = sizeBytes;
			Version = version;
			IsEncrypted = isEncrypted;
			Dependencies = dependencies;
		}
	}
}