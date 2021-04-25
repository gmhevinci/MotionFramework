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
	public class PatchBundle
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 文件哈希值
		/// </summary>
		public string Hash;

		/// <summary>
		/// 文件校验码
		/// </summary>
		public string CRC;

		/// <summary>
		/// 文件大小（字节数）
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
		/// 收集的资源列表
		/// </summary>
		public string[] CollectAssets;

		/// <summary>
		/// 依赖的资源包ID列表
		/// </summary>
		public int[] DependIDs;

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
		/// 依赖的资源包名称列表
		/// </summary>
		[NonSerialized]
		public string[] Depends;


		public PatchBundle(string bundleName, string hash, string crc, long sizeBytes, int version, int flags, string[] collectAssets, string[] depends, string[] dlcLabels)
		{
			BundleName = bundleName;
			Hash = hash;
			CRC = crc;
			SizeBytes = sizeBytes;
			Version = version;
			Flags = flags;
			CollectAssets = collectAssets;
			Depends = depends;
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
		public static int CreateFlags(bool isEncrypted)
		{
			BitMask32 flags = new BitMask32(0);
			if (isEncrypted) flags.Open(0);
			return flags;
		}

		/// <summary>
		/// 解析标记位
		/// </summary>
		public static void ParseFlags(int flags, out bool isEncrypted)
		{
			BitMask32 value = flags;
			isEncrypted = value.Test(0);
		}
	}
}