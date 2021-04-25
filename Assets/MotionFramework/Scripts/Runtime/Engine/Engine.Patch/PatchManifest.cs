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
		/// 资源包列表
		/// </summary>
		public List<PatchBundle> BundleList = new List<PatchBundle>();

		/// <summary>
		/// 变体列表
		/// </summary>
		public List<PatchVariant> VariantList = new List<PatchVariant>();


		/// <summary>
		/// 资源包集合（提供AssetBundle名称获取PatchBundle）
		/// </summary>
		[NonSerialized]
		public readonly Dictionary<string, PatchBundle> Bundles = new Dictionary<string, PatchBundle>();

		/// <summary>
		/// 变体集合（提供AssetBundle名称获取PatchVariant）
		/// </summary>
		[NonSerialized]
		private readonly Dictionary<string, PatchVariant> Variants = new Dictionary<string, PatchVariant>();

		/// <summary>
		/// 资源映射集合（提供AssetPath获取PatchBundle）
		/// </summary>
		[NonSerialized]
		private readonly Dictionary<string, PatchBundle> AssetsMapping = new Dictionary<string, PatchBundle>();

		
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
			if (Bundles.TryGetValue(bundleName, out PatchBundle value))
			{
				return value.Depends;
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
			if (AssetsMapping.TryGetValue(assetPath, out PatchBundle value))
			{
				return value.BundleName;
			}
			else
			{
				MotionLog.Error($"Not found asset in patch manifest : {assetPath}");
				return string.Empty;
			}
		}


		/// <summary>
		/// 序列化
		/// </summary>
		public static void Serialize(string savePath, PatchManifest patchManifest)
		{
			// 构建依赖关系
			foreach (var patchBundle in patchManifest.BundleList)
			{
				patchBundle.DependIDs = GetPatchDpendIDs(patchManifest, patchBundle);
			}

			string json = JsonUtility.ToJson(patchManifest);
			FileUtility.CreateFile(savePath, json);
		}
		private static int[] GetPatchDpendIDs(PatchManifest patchManifest, PatchBundle patchBundle)
		{
			List<int> result = new List<int>();
			foreach (var bundleName in patchBundle.Depends)
			{
				int dependID = GetPatchDependID(patchManifest, bundleName);
				result.Add(dependID);
			}
			return result.ToArray();
		}
		private static int GetPatchDependID(PatchManifest patchManifest, string bundleName)
		{
			for (int i = 0; i < patchManifest.BundleList.Count; i++)
			{
				var patchBundle = patchManifest.BundleList[i];
				if (patchBundle.BundleName == bundleName)
					return i;
			}
			throw new Exception($"Not found bundle {bundleName}");
		}

		/// <summary>
		/// 反序列化
		/// </summary>
		public static PatchManifest Deserialize(string jsonData)
		{
			PatchManifest patchManifest = JsonUtility.FromJson<PatchManifest>(jsonData);

			// 构建资源包集合
			foreach (var patchBundle in patchManifest.BundleList)
			{
				patchManifest.Bundles.Add(patchBundle.BundleName, patchBundle);

				// 解析标记位
				PatchBundle.ParseFlags(patchBundle.Flags, out patchBundle.IsEncrypted);

				// 解析依赖列表
				patchBundle.Depends = GetDepends(patchManifest, patchBundle);

				// 构建资源映射集合
				UpdateAssetMap(patchManifest, patchBundle);
			}

			// 构建变种集合
			foreach (var variant in patchManifest.VariantList)
			{
				patchManifest.Variants.Add(variant.BundleName, variant);
			}

			return patchManifest;
		}
		private static string[] GetDepends(PatchManifest patchManifest, PatchBundle patchBundle)
		{
			List<string> result = new List<string>(patchBundle.DependIDs.Length);
			foreach (var dependID in patchBundle.DependIDs)
			{
				if (dependID >= 0 && dependID < patchManifest.BundleList.Count)
				{
					var dependPatchBundle = patchManifest.BundleList[dependID];
					result.Add(dependPatchBundle.BundleName);
				}
				else
				{
					throw new Exception($"Invalid depend id : {dependID} : {patchBundle.BundleName}");
				}
			}
			return result.ToArray();
		}
		private static void UpdateAssetMap(PatchManifest patchManifest, PatchBundle patchBundle)
		{
			// 构建主动收集的资源路径和资源包之间的映射关系。
			// 注意：这里面不包括依赖的非主动收集资源。
			foreach (var assetPath in patchBundle.CollectAssets)
			{
				// 添加原始路径
				// 注意：我们不允许原始路径存在重名
				if (patchManifest.AssetsMapping.ContainsKey(assetPath))
					throw new Exception($"Asset path have existed : {assetPath}");
				patchManifest.AssetsMapping.Add(assetPath, patchBundle);

				// 添加去掉后缀名的路径
				if (Path.HasExtension(assetPath))
				{
					string assetPathWithoutExtension = assetPath.RemoveExtension();
					if (patchManifest.AssetsMapping.ContainsKey(assetPathWithoutExtension))
						MotionLog.Warning($"Asset path have existed : {assetPathWithoutExtension}");
					else
						patchManifest.AssetsMapping.Add(assetPathWithoutExtension, patchBundle);
				}
			}
		}
	}
}