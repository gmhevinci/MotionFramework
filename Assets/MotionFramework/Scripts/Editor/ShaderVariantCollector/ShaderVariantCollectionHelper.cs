//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 lujian101
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace MotionFramework.Editor
{
	public static class ShaderVariantCollectionHelper
	{
		public static Dictionary<Shader, List<ShaderVariantCollection.ShaderVariant>> Extract(ShaderVariantCollection svc)
		{
			var result = new Dictionary<Shader, List<ShaderVariantCollection.ShaderVariant>>(1000);
			using (var so = new SerializedObject(svc))
			{
				var shaderArray = so.FindProperty("m_Shaders.Array");
				if (shaderArray != null && shaderArray.isArray)
				{
					for (int i = 0; i < shaderArray.arraySize; ++i)
					{
						var shaderRef = shaderArray.FindPropertyRelative($"data[{i}].first");
						var shaderVariantsArray = shaderArray.FindPropertyRelative($"data[{i}].second.variants");
						if (shaderRef != null && shaderRef.propertyType == SerializedPropertyType.ObjectReference && shaderVariantsArray != null && shaderVariantsArray.isArray)
						{
							var shader = shaderRef.objectReferenceValue as Shader;
							if (shader == null)
								continue;

							string shaderAssetPath = AssetDatabase.GetAssetPath(shader);

							// 添加着色器
							if (result.TryGetValue(shader, out List<ShaderVariantCollection.ShaderVariant> variants) == false)
							{
								variants = new List<ShaderVariantCollection.ShaderVariant>();
								result.Add(shader, variants);
							}

							// 添加变种信息
							for (int j = 0; j < shaderVariantsArray.arraySize; ++j)
							{
								var propKeywords = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].keywords");
								var propPassType = shaderVariantsArray.FindPropertyRelative($"Array.data[{j}].passType");
								if (propKeywords != null && propPassType != null && propKeywords.propertyType == SerializedPropertyType.String)
								{
									string[] keywords = propKeywords.stringValue.Split(' ');
									PassType pathType = (PassType)propPassType.intValue;
									variants.Add(new ShaderVariantCollection.ShaderVariant(shader, pathType, keywords));
								}
							}
						}
					}
				}
			}
			return result;
		}
	}
}