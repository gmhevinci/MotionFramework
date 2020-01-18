//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Patch
{
	/// <summary>
	/// 补丁清单文件
	/// </summary>
	public class PatchManifest
	{
		private bool _isParse = false;

		/// <summary>
		/// 资源版本号
		/// </summary>
		public int Version { private set; get; }

		/// <summary>
		/// 所有打包文件列表
		/// </summary>
		public readonly Dictionary<string, PatchElement> Elements = new Dictionary<string, PatchElement>();


		/// <summary>
		/// 解析数据
		/// </summary>
		public void Parse(string text)
		{
			if (string.IsNullOrEmpty(text))
				throw new Exception("Fatal error : Param is null or empty");
			if (_isParse)
				throw new Exception("Fatal error : Package is already parse.");

			_isParse = true;

			// 按行分割字符串
			string[] lineArray = text.Split('\n');

			// 读取版本号
			Version = int.Parse(lineArray[0]);

			// 读取所有Bundle的数据
			for (int i = 1; i < lineArray.Length; i++)
			{
				string line = lineArray[i];
				if (string.IsNullOrEmpty(line))
					continue;

				string[] splits = line.Split('=');
				string fileName = splits[0];
				string fileMD5 = splits[1];
				long fileSizeKB = long.Parse(splits[2]);
				int fileVersion = int.Parse(splits[3]);

				if (Elements.ContainsKey(fileName))
					throw new Exception($"Fatal error : has same pack file : {fileName}");
				Elements.Add(fileName, new PatchElement(fileName, fileMD5, fileVersion, fileSizeKB));
			}
		}

		/// <summary>
		/// 解析数据
		/// </summary>
		public void Parse(StreamReader sr)
		{
			if (sr == null)
				throw new Exception("Fatal error : Param is null.");
			if (_isParse)
				throw new Exception("Fatal error : Package is already parse.");

			_isParse = true;

			// 读取版本号
			Version = int.Parse(sr.ReadLine());

			// 读取所有Bundle的数据
			while (true)
			{
				string content = sr.ReadLine();
				if (content == null)
					break;

				string[] splits = content.Split('=');
				string fileName = splits[0];
				string fileMD5 = splits[1];
				long fileSizeKB = long.Parse(splits[2]);
				int fileVersion = int.Parse(splits[3]);

				if (Elements.ContainsKey(fileName))
					throw new Exception($"Fatal error : has same pack file : {fileName}");
				Elements.Add(fileName, new PatchElement(fileName, fileMD5, fileVersion, fileSizeKB));
			}
		}
	}
}