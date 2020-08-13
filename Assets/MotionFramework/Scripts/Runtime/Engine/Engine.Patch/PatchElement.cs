//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Linq;

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
		/// 文件校验码
		/// </summary>
		public uint CRC32;

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
		/// DLC标签列表
		/// </summary>
		public string[] DLCLabels;

		public PatchElement(string name, string md5, uint crc32, long sizeBytes, int version, bool isEncrypted, string[] dependencies, string[] dlcLabels)
		{
			Name = name;
			MD5 = md5;
			CRC32 = crc32;
			SizeBytes = sizeBytes;
			Version = version;
			IsEncrypted = isEncrypted;
			Dependencies = dependencies;
			DLCLabels = dlcLabels;
		}

		/// <summary>
		/// 是否为DLC资源
		/// </summary>
		public bool IsDLC()
		{
			return DLCLabels != null && DLCLabels.Length > 0;
		}

		/// <summary>
		/// 是否包含DLC标签
		/// </summary>
		public bool HasDLCLabel(string label)
		{
			if (DLCLabels == null)
				return false;
			return DLCLabels.Contains(label);
		}

		/// <summary>
		/// 是否包含DLC标签
		/// </summary>
		public bool HasDLCLabel(string[] labels)
		{
			if (DLCLabels == null)
				return false;
			for (int i = 0; i < labels.Length; i++)
			{
				if (DLCLabels.Contains(labels[i]))
					return true;
			}
			return false;
		}
	}
}