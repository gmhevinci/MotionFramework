//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
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
		/// 元素集合（提供AssetBundle名称获取PatchElement）
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchElement> Elements = new Dictionary<string, PatchElement>();

		/// <summary>
		/// 变体集合（提供AssetBundle名称获取PatchVariant）
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchVariant> Variants = new Dictionary<string, PatchVariant>();

		/// <summary>
		/// 资源映射集合（提供AssetPath获取PatchElement）
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
			if (Variants.TryGetValue(bundleName, out PatchVariant value))
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
			if (Elements.TryGetValue(bundleName, out PatchElement value))
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

			// 解析元素列表
			foreach (var element in patchManifest.ElementList)
			{
				// 解析标记位
				PatchElement.ParseFlags(element.Flags, out element.IsEncrypted, out element.IsCollected);

				// 元素集合
				patchManifest.Elements.Add(element.BundleName, element);

				// 注意：直接跳过非收集文件，因为这些文件不需要代码加载
				if (element.IsCollected == false)
					continue;

				// 资源映射集合
				foreach (var assetPath in element.AssetPaths)
				{
					// 添加原始路径
					// 注意：我们不允许原始路径存在重名
					if (patchManifest.AssetsMap.ContainsKey(assetPath))
						throw new Exception($"Asset path have existed : {assetPath}");
					patchManifest.AssetsMap.Add(assetPath, element);

					// 添加去掉后缀名的路径
					if (Path.HasExtension(assetPath))
					{
						string assetPathWithoutExtension = assetPath.RemoveExtension();
						if (patchManifest.AssetsMap.ContainsKey(assetPathWithoutExtension))
							MotionLog.Warning($"Asset path have existed : {assetPathWithoutExtension}");
						else
							patchManifest.AssetsMap.Add(assetPathWithoutExtension, element);
					}
				}
			}

			// 解析变种列表
			foreach (var variant in patchManifest.VariantList)
			{
				patchManifest.Variants.Add(variant.BundleName, variant);
			}

			return patchManifest;
		}
	}
}