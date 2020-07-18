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
		/// 下载文件的保存路径
		/// </summary>
		[NonSerialized]
		public string SavePath;

		public PatchElement(string name, string md5, long sizeBytes, int version)
		{
			Name = name;
			MD5 = md5;
			SizeBytes = sizeBytes;
			Version = version;
		}
	}
}