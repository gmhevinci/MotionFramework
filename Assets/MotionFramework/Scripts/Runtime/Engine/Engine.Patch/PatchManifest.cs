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
		/// <summary>
		/// 资源版本号
		/// </summary>
		public int ResourceVersion;

		/// <summary>
		/// 元素列表
		/// </summary>
		public List<PatchElement> ElementList = new List<PatchElement>();

		/// <summary>
		/// 变体列表
		/// </summary>
		public List<PatchVariant> VariantList = new List<PatchVariant>();

		/// <summary>
		/// 元素集合
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchElement> Elements = new Dictionary<string, PatchElement>();

		/// <summary>
		/// 变体集合
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchVariant> Variants = new Dictionary<string, PatchVariant>();

		/// <summary>
		/// 资源映射集合
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchElement> AssetsMap = new Dictionary<string, PatchElement>();


		/// <summary>
		/// 是否包含变体资源
		/// </summary>
		public bool HasVariant(string bundleName)
		{
			return Variants.ContainsKey(bundleName);
		}

		/// <summary>
		/// 获取首个变体格式
		/// </summary>
		public string GetFirstVariant(string bundleName)
		{
			if(Variants.TryGetValue(bundleName, out PatchVariant value))
			{
				return value.Variants[0];
			}
			return string.Empty;
		}

		/// <summary>
		/// 获取资源依赖列表
		/// </summary>
		public string[] GetDirectDependencies(string bundleName)
		{
			if(Elements.TryGetValue(bundleName, out PatchElement value))
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
		public string[] GetAllDependencies(string bundleName)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// 获取资源包名称
		/// </summary>
		public string GetAssetBundleName(string assetPath)
		{
			if (AssetsMap.TryGetValue(assetPath, out PatchElement value))
			{
				return value.BundleName;
			}
			else
			{
				MotionLog.Error($"Not found asset in patch manifest: {assetPath}");
				return string.Empty;
			}
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
				patchManifest.Elements.Add(element.BundleName, element);
				foreach(var assetPath in element.AssetPaths)
				{
					if (patchManifest.AssetsMap.ContainsKey(assetPath))
						throw new Exception($"Asset path have existed : {assetPath}");
					patchManifest.AssetsMap.Add(assetPath, element);
				}
			}

			foreach (var variant in patchManifest.VariantList)
			{
				patchManifest.Variants.Add(variant.BundleName, variant);
			}

			return patchManifest;
		}
	}
}