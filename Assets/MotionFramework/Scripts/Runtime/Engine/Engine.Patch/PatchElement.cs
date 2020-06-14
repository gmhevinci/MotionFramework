//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

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
		/// 文件大小
		/// </summary>
		public long SizeBytes { private set; get; }

		/// <summary>
		/// 文件版本
		/// </summary>
		public int Version { private set; get; }

		/// <summary>
		/// 变体类型列表
		/// </summary>
		public List<string> Variants { private set; get; }

		/// <summary>
		/// 下载文件的保存路径
		/// </summary>
		public string SavePath;


		public PatchElement(string name, string md5, long sizeBytes, int version, List<string> variants)
		{
			Name = name;
			MD5 = md5;
			SizeBytes = sizeBytes;
			Version = version;
			Variants = variants;
		}

		/// <summary>
		/// 是否包含变体资源
		/// </summary>
		public bool HasVariant()
		{
			if (Variants == null || Variants.Count == 0)
				return false;
			else
				return true;
		}

		/// <summary>
		/// 获取首个变体类型，如果不存在返回一个空字符串
		/// </summary>
		public string GetFirstVariant()
		{
			if (HasVariant() == false)
				return string.Empty;
			return Variants[0];
		}
	}
}