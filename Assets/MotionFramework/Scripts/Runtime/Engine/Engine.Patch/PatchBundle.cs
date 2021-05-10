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
		/// 收集的资源列表
		/// </summary>
		public string[] CollectAssets;

		/// <summary>
		/// 依赖的资源包ID列表
		/// </summary>
		public int[] DependIDs;

		/// <summary>
		/// Tags
		/// </summary>
		public string[] Tags;

		/// <summary>
		/// Flags
		/// </summary>
		public int Flags;


		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncrypted { private set; get; }

		/// <summary>
		/// 是否为内置文件
		/// </summary>
		public bool IsBuildin { private set; get; }

		/// <summary>
		/// 依赖的资源包名称列表
		/// </summary>
		public string[] Depends { set; get; }


		public PatchBundle(string bundleName, string hash, string crc, long sizeBytes, int version, string[] collectAssets, string[] depends, string[] tags)
		{
			BundleName = bundleName;
			Hash = hash;
			CRC = crc;
			SizeBytes = sizeBytes;
			Version = version;
			CollectAssets = collectAssets;
			Depends = depends;
			Tags = tags;
		}

		/// <summary>
		/// 设置Flags
		/// </summary>
		public void SetFlagsValue(bool isEncrypted, bool isBuildin)
		{
			IsEncrypted = isEncrypted;
			IsBuildin = isBuildin;

			BitMask32 mask = new BitMask32(0);
			if (isEncrypted) mask.Open(0);
			if (isBuildin) mask.Open(1);
			Flags = mask;
		}

		/// <summary>
		/// 解析Flags
		/// </summary>
		public void ParseFlagsValue()
		{
			BitMask32 value = Flags;
			IsEncrypted = value.Test(0);
			IsBuildin = value.Test(1);
		}

		/// <summary>
		/// 是否包含Tag
		/// </summary>
		public bool HasTag(string[] tags)
		{
			if (Tags == null || Tags.Length == 0)
				return false;
			foreach (var tag in tags)
			{
				if (Tags.Contains(tag))
					return true;
			}
			return false;
		}
	}
}