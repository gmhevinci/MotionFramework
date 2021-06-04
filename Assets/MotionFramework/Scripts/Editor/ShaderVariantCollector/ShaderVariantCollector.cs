//--------------------------------------------------
// Motion Framework
// Copyright©2019-2019 https://github.com/networm
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	public class ShaderVariantCollector
	{
		/// <summary>
		/// 开始构建
		/// </summary>
		public void Run(string saveFilePath)
		{
			if (Path.HasExtension(saveFilePath) == false)
				saveFilePath = $"{saveFilePath}.shadervariants";

			if (Path.GetExtension(saveFilePath) != ".shadervariants")
				throw new System.Exception("Shader variant file extension is invalid.");

			EditorTools.CreateFileDirectory(saveFilePath);
			var materials = GetAllMaterials();
			ClearCurrentShaderVariantCollection();
			CollectVariants(materials);
			SaveCurrentShaderVariantCollection(saveFilePath);
		}

		/// <summary>
		/// 收集所有打包的材质球
		/// </summary>
		private List<Material> GetAllMaterials()
		{
			int progressValue = 0;
			List<string> allAssets = new List<string>(1000);

			// 获取所有打包的资源
			List<AssetCollectInfo> allCollectInfos = AssetBundleCollectorSettingData.GetAllCollectAssets();
			List<string> collectAssets = allCollectInfos.Select(t => t.AssetPath).ToList();
			foreach (var assetPath in collectAssets)
			{
				string[] depends = AssetDatabase.GetDependencies(assetPath, true);
				foreach (var depend in depends)
				{
					if (allAssets.Contains(depend) == false)
						allAssets.Add(depend);
				}
				EditorTools.DisplayProgressBar("获取所有打包资源", ++progressValue, collectAssets.Count);
			}
			EditorTools.ClearProgressBar();

			// 搜集所有材质球
			progressValue = 0;
			var shaderDic = new Dictionary<Shader, List<Material>>(100);
			foreach (var assetPath in allAssets)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Material))
				{
					var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
					var shader = material.shader;
					if (shader == null)
						continue;

					if (shaderDic.ContainsKey(shader) == false)
					{
						shaderDic.Add(shader, new List<Material>());
					}
					if (shaderDic[shader].Contains(material) == false)
					{
						shaderDic[shader].Add(material);
					}
				}
				EditorTools.DisplayProgressBar("搜集所有材质球", ++progressValue, allAssets.Count);
			}
			EditorTools.ClearProgressBar();

			// 返回结果
			var materials = new List<Material>(1000);
			foreach (var valuePair in shaderDic)
			{
				materials.AddRange(valuePair.Value);
			}
			return materials;
		}

		/// <summary>
		/// 采集所有着色器的变种
		/// </summary>
		private void CollectVariants(List<Material> materials)
		{
			// 创建临时场景
			EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

			// 设置主相机
			Camera camera = Camera.main;
			float aspect = camera.aspect;
			int totalMaterials = materials.Count;
			float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
			float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
			float halfHeight = Mathf.CeilToInt(height / 2f);
			float halfWidth = Mathf.CeilToInt(width / 2f);
			camera.orthographic = true;
			camera.orthographicSize = halfHeight;
			camera.transform.position = new Vector3(0f, 0f, -10f);
			Selection.activeGameObject = camera.gameObject;
			EditorApplication.ExecuteMenuItem("GameObject/Align View to Selected");

			// 创建测试球体
			int xMax = (int)(width - 1);
			int x = 0, y = 0;
			int progressValue = 0;
			for (int i = 0; i < materials.Count; i++)
			{
				var material = materials[i];
				var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
				CreateSphere(material, position, i);
				if (x == xMax)
				{
					x = 0;
					y++;
				}
				else
				{
					x++;
				}
				EditorTools.DisplayProgressBar("测试所有材质球", ++progressValue, materials.Count);
			}
			EditorTools.ClearProgressBar();
		}

		/// <summary>
		/// 创建测试球体
		/// </summary>
		private static void CreateSphere(Material material, Vector3 position, int index)
		{
			var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			go.GetComponent<Renderer>().material = material;
			go.transform.position = position;
			go.name = $"Sphere_{index}|{material.name}";
		}

		private void ClearCurrentShaderVariantCollection()
		{
			AssemblyUtility.InvokeInternalStaticMethod(typeof(ShaderUtil), "ClearCurrentShaderVariantCollection");
		}
		private void SaveCurrentShaderVariantCollection(string saveFilePath)
		{
			//AssemblyUtility.InvokeInternalStaticMethod(typeof(ShaderUtil), "SaveCurrentShaderVariantCollection", saveFilePath);
		}
		public int GetCurrentShaderVariantCollectionShaderCount()
		{
			return (int)AssemblyUtility.InvokeInternalStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionShaderCount");
		}
		public int GetCurrentShaderVariantCollectionVariantCount()
		{
			return (int)AssemblyUtility.InvokeInternalStaticMethod(typeof(ShaderUtil), "GetCurrentShaderVariantCollectionVariantCount");
		}
	}
}