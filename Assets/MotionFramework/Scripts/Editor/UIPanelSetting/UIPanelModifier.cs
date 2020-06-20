//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

namespace MotionFramework.Editor
{
	public static class UIPanelModifier
	{
		/// <summary>
		/// 刷新面板清单
		/// </summary>
		public static void Refresh(UIManifest manifest)
		{
			if(UIPanelSettingData.CheckValid())
			{
				CacheUIElement(manifest);
				UpdateUIComponent(manifest);
			}
		}

		/// <summary>
		/// 缓存所有UI元素
		/// </summary>
		private static void CacheUIElement(UIManifest manifest)
		{
			Transform root = manifest.transform;

			// 清空旧数据
			manifest.ElementPath.Clear();
			manifest.ElementTrans.Clear();

			Transform[] allTrans = root.GetComponentsInChildren<Transform>(true);
			for (int i = 0; i < allTrans.Length; i++)
			{
				Transform trans = allTrans[i];
				string path = GetFullPath(root, trans);
				AddElementToList(manifest, path, trans);
			}

			Debug.Log($"Cache panel {root.name} total {allTrans.Length} elements");
		}

		/// <summary>
		/// 获取到根节点的全路径
		/// </summary>
		private static string GetFullPath(Transform root, Transform trans)
		{
			string path = trans.name;
			while (trans.parent != null)
			{
				// 如果找到了根节点
				if (trans == root)
				{
					break;
				}
				else
				{
					trans = trans.parent;
					if (trans != null)
						path = trans.name + "/" + path;
				}
			}
			return path;
		}

		/// <summary>
		/// 添加一个UI元素到列表
		/// </summary>
		private static void AddElementToList(UIManifest manifest, string path, Transform trans)
		{
			if (string.IsNullOrEmpty(path) || trans == null)
				throw new System.NullReferenceException();

			// 如果有重复路径的元素
			for (int i = 0; i < manifest.ElementPath.Count; i++)
			{
				if (manifest.ElementPath[i] == path)
				{
					Debug.LogError($"发现重复路径的元素 : {path}");
					return;
				}
			}

			manifest.ElementPath.Add(path);
			manifest.ElementTrans.Add(trans);
		}

		/// <summary>
		/// 更新组件
		/// </summary>
		private static void UpdateUIComponent(UIManifest manifest)
		{
			Transform root = manifest.transform;

			// Clear cache
			manifest.CacheAtlasTags.Clear();

			// 获取依赖的图集名称
			Image[] allImage = root.GetComponentsInChildren<Image>(true);
			for (int i = 0; i < allImage.Length; i++)
			{
				Image img = allImage[i];

				// Clear
				UISprite uiSprite = img.GetComponent<UISprite>();
				if (uiSprite != null)
					uiSprite.Atlas = null;

				// 如果图片为空
				if (img.sprite == null)
					continue;

				// 文件路径
				string assetPath = UnityEditor.AssetDatabase.GetAssetPath(img.sprite);

				// 如果是系统内置资源
				if (assetPath.Contains("_builtin_"))
					continue;

				// 如果是图集资源
				string spriteDirectory = UIPanelSettingData.Setting.UISpriteDirectory;
				if (assetPath.Contains(spriteDirectory))
				{
					if (uiSprite == null)
						uiSprite = img.gameObject.AddComponent<UISprite>();

					string atlasAssetPath = GetAtlasPath(assetPath);
					SpriteAtlas spriteAtlas = UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasAssetPath);
					if (spriteAtlas == null)
					{
						throw new System.Exception($"Not found SpriteAtlas : {atlasAssetPath}");
					}
					else
					{
						uiSprite.Atlas = spriteAtlas;
						string atlasName = Path.GetFileNameWithoutExtension(atlasAssetPath);
						if (manifest.CacheAtlasTags.Contains(atlasName) == false)
							manifest.CacheAtlasTags.Add(atlasName);
					}
				}
			}
		}

		/// <summary>
		/// 获取精灵所属图集
		/// </summary>
		private static string GetAtlasPath(string assetPath)
		{
			string spriteDirectory = UIPanelSettingData.Setting.UISpriteDirectory;
			string atlasDirectory = UIPanelSettingData.Setting.UIAtlasDirectory;

			// 获取图片所在总文件下的子文件夹
			string temp = assetPath.Replace(spriteDirectory, string.Empty);
			string[] splits = temp.Split('/');
			string folderName = splits[1];

			return $"{atlasDirectory}/{folderName}.spriteatlas";
		}
	}
}