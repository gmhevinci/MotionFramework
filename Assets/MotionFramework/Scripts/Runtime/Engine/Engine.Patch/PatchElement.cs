//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Linq;
using MotionFramework.Utility;

namespace MotionFramework.Patch
{
	[Serializable]
	public class PatchElement
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

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
		/// 标记位
		/// </summary>
		public int Flags;

		/// <summary>
		/// 依赖列表
		/// </summary>
		public string[] Dependencies;

		/// <summary>
		/// 资源对象列表
		/// </summary>
		public string[] AssetPaths;

		/// <summary>
		/// DLC标签列表
		/// </summary>
		public string[] DLCLabels;

		/// <summary>
		/// 是否为加密文件
		/// </summary>
		[NonSerialized]
		public bool IsEncrypted;

		/// <summary>
		/// 是否为收集文件
		/// </summary>
		[NonSerialized]
		public bool IsCollected;


		public PatchElement(string bundleName, string md5, uint crc32, long sizeBytes, int version, int flags, string[] assetPaths, string[] dependencies, string[] dlcLabels)
		{
			BundleName = bundleName;
			MD5 = md5;
			CRC32 = crc32;
			SizeBytes = sizeBytes;
			Version = version;
			Flags = flags;
			AssetPaths = assetPaths;
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


		/// <summary>
		/// 创建标记位
		/// </summary>
		/// <param name="isEncrypted">是否为加密文件</param>
		/// <param name="isCollected">是否为收集文件</param>
		public static int CreateFlags(bool isEncrypted, bool isCollected)
		{
			BitMask32 flags = new BitMask32(0);
			if (isEncrypted) flags.Open(0);
			if (isCollected) flags.Open(1);
			return flags;
		}

		/// <summary>
		/// 解析标记位
		/// </summary>
		public static void ParseFlags(int flags, out bool isEncrypted, out bool isCollected)
		{
			BitMask32 value = flags;
			isEncrypted = value.Test(0);
			isCollected = value.Test(1);
		}
	}
}