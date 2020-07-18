//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Utility;

namespace MotionFramework.Patch
{
	/// <summary>
	/// 补丁清单文件
	/// </summary>
	[Serializable]
	public class PatchManifest
	{
		public const int FileStreamMaxLen = 1024 * 1024 * 128; //最大128MB
		public const int TableStreamMaxLen = 1024 * 256; //最大256K
		public const short TableStreamHead = 0x2B2B; //文件标记
	
		/// <summary>
		/// 资源版本号
		/// </summary>
		public int ResourceVersion;

		/// <summary>
		/// 资源列表
		/// </summary>
		public List<PatchElement> ElementList = new List<PatchElement>();

		/// <summary>
		/// 变体列表
		/// </summary>
		public List<PatchVariant> VariantList = new List<PatchVariant>();

		/// <summary>
		/// 资源集合
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchElement> Elements = new Dictionary<string, PatchElement>();

		/// <summary>
		/// 变体集合
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchVariant> Variants = new Dictionary<string, PatchVariant>();


		/// <summary>
		/// 是否包含变体资源
		/// </summary>
		public bool HasVariant(string name)
		{
			return Variants.ContainsKey(name);
		}

		/// <summary>
		/// 获取首个变体格式
		/// </summary>
		public string GetFirstVariant(string name)
		{
			if(Variants.TryGetValue(name, out PatchVariant value))
			{
				return value.Variants[0];
			}
			return string.Empty;
		}

		/// <summary>
		/// 获取资源依赖列表
		/// </summary>
		public string[] GetDirectDependencies(string name)
		{
			if(Elements.TryGetValue(name, out PatchElement value))
			{
				return value.Dependencies;
			}
			else
			{
				return new string[] { };
			}
		}

		/// <summary>
		/// 获取资源依赖列表
		/// </summary>
		public string[] GetAllDependencies(string name)
		{
			throw new NotImplementedException();
		}


		/// <summary>
		/// 序列化
		/// </summary>
		public static void Serialize(string savePath, PatchManifest obj)
		{
			string json = JsonUtility.ToJson(obj);
			FileUtility.CreateFile(savePath, json);
		}

		/// <summary>
		/// 反序列化
		/// </summary>
		public static PatchManifest Deserialize(string jsonData)
		{
			PatchManifest patchManifest = JsonUtility.FromJson<PatchManifest>(jsonData);
			foreach (var element in patchManifest.ElementList)
			{
				patchManifest.Elements.Add(element.Name, element);
			}
			foreach (var variant in patchManifest.VariantList)
			{
				patchManifest.Variants.Add(variant.Name, variant);
			}
			return patchManifest;
		}
	}
}