//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.IO;

namespace MotionFramework.Patch
{
	/// <summary>
	/// 补丁清单文件
	/// </summary>
	public class PatchManifest
	{
		public const int FileStreamMaxLen = 1024 * 1024 * 128; //最大128MB
		public const int TableStreamMaxLen = 1024 * 256; //最大256K
		public const short TableStreamHead = 0x2B2B; //文件标记

		private bool _isParse = false;

		/// <summary>
		/// 资源版本号
		/// </summary>
		public int Version { private set; get; }

		/// <summary>
		/// 所有资源集合
		/// </summary>
		public Dictionary<string, PatchElement> Elements { get; private set; } = new Dictionary<string, PatchElement>();

		/// <summary>
		/// 解析数据
		/// </summary>
		public void Parse(byte[] bytes)
		{
			if (bytes == null)
				throw new Exception("Fatal error : Param is null.");
			if (_isParse)
				throw new Exception("Fatal error : Package is already parse.");

			_isParse = true;

			// 字节缓冲区
			ByteBuffer bb = new ByteBuffer(bytes);

			// 读取版本号
			Version = bb.ReadInt();

			// 读取元素总数
			int elementCount = bb.ReadInt();
			Elements = new Dictionary<string, PatchElement>(elementCount);

			int tableLine = 1;
			const int headMarkAndSize = 6; //注意：short字节数+int字节数
			while (bb.IsReadable(headMarkAndSize))
			{
				// 检测行标记
				short tableHead = bb.ReadShort();
				if (tableHead != TableStreamHead)
				{
					throw new Exception($"PatchManifest table stream head is invalid. Table line is {tableLine}");
				}

				// 检测行大小
				int tableSize = bb.ReadInt();
				if (!bb.IsReadable(tableSize) || tableSize > TableStreamMaxLen)
				{
					throw new Exception($"PatchManifest table stream size is invalid. Table size is {tableSize},Table line {tableLine}");
				}

				// 读取行内容
				string fileName = bb.ReadUTF();
				string fileMD5 = bb.ReadUTF();
				long fileSizeBytes = bb.ReadLong();
				int fileVersion = bb.ReadInt();
				List<string> variantList = bb.ReadListUTF();

				// 添加到集合
				if (Elements.ContainsKey(fileName))
					throw new Exception($"Fatal error : PatchManifest has same element : {fileName}");
				Elements.Add(fileName, new PatchElement(fileName, fileMD5, fileSizeBytes, fileVersion, variantList));

				++tableLine;
			}
		}
	}
}