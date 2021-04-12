//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.IO;

namespace MotionFramework.Patch
{
	internal class VariantCollector
	{
		private readonly Dictionary<string, string> _variantRuleCollection = new Dictionary<string, string>(1000);
		private readonly Dictionary<string, string> _cacheNames = new Dictionary<string, string>(1000);

		/// <summary>
		/// 注册变体规则
		/// </summary>
		/// <param name="variantGroup">变体组</param>
		/// <param name="targetVariant">目标变体</param>
		public void RegisterVariantRule(List<string> variantGroup, string targetVariant)
		{
			if (variantGroup == null || variantGroup.Count == 0)
				throw new Exception("VariantGroup is null or empty.");
			if (string.IsNullOrEmpty(targetVariant))
				throw new Exception("TargetVariant is null or empty.");

			// 规则处理
			for (int i = 0; i < variantGroup.Count; i++)
			{
				variantGroup[i] = variantGroup[i].ToLower();
				if (variantGroup[i].StartsWith("."))
					variantGroup[i] = variantGroup[i].RemoveFirstChar();
			}

			// 规则处理
			{
				targetVariant = targetVariant.ToLower();
				if (targetVariant.StartsWith("."))
					targetVariant = targetVariant.RemoveFirstChar();
			}

			// 注意：目标变体类型需要在变体类型列表里
			if (targetVariant != VariantRule.DefaultTag)
			{			
				if (variantGroup.Contains(targetVariant) == false)
					throw new Exception($"Variant group not contains target variant : {targetVariant} ");
			}

			foreach (var variant in variantGroup)
			{
				if (_variantRuleCollection.ContainsKey(variant) == false)
					_variantRuleCollection.Add(variant, targetVariant);
				else
					MotionLog.Warning($"Variant key {variant} is already existed.");
			}
		}

		/// <summary>
		/// Remaps the asset bundle name to the target variant.
		/// </summary>
		public string RemapVariantName(PatchManifest patchManifest, string bundleName)
		{
			if (patchManifest.HasVariant(bundleName))
			{
				string variant = patchManifest.GetFirstVariant(bundleName);
				return GetCachedVariantBundleName(bundleName, variant);
			}
			return bundleName;
		}

		/// <summary>
		/// 获取缓存的变体资源包名称
		/// </summary>
		private string GetCachedVariantBundleName(string bundleName, string variant)
		{
			if (_cacheNames.ContainsKey(bundleName))
				return _cacheNames[bundleName];

			// 获取变体资源格式
			string variantBundleName = bundleName;
			if (_variantRuleCollection.ContainsKey(variant))
			{
				string extension = _variantRuleCollection[variant];
				if (extension == VariantRule.DefaultTag)
					extension = PatchDefine.AssetBundleDefaultVariant;
				string filePathWithoutExtension = bundleName.RemoveExtension();
				variantBundleName = StringFormat.Format("{0}.{1}", filePathWithoutExtension, extension);
			}
			else
			{
				MotionLog.Warning($"Not found variant in rules : {variant}");
			}

			// 添加到缓存列表
			_cacheNames.Add(bundleName, variantBundleName);
			return variantBundleName;
		}
	}
}