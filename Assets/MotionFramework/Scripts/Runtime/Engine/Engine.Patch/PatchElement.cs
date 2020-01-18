//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Patch
{
	public class PatchElement
	{
		/// <summary>
		/// 文件名称
		/// </summary>
		public string Name { private set; get; }

		/// <summary>
		/// 文件MD5
		/// </summary>
		public string MD5 { private set; get; }

		/// <summary>
		/// 文件版本
		/// </summary>
		public int Version { private set; get; }

		/// <summary>
		/// 文件大小
		/// </summary>
		public long SizeKB { private set; get; }

		/// <summary>
		/// 下载文件的保存路径
		/// </summary>
		public string SavePath;


		public PatchElement(string name, string md5, int version, long sizeKB)
		{
			Name = name;
			MD5 = md5;
			Version = version;
			SizeKB = sizeKB;
		}
	}
}