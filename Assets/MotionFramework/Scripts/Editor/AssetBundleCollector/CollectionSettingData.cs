//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public static class CollectionSettingData
	{
		public static CollectionSetting Setting;

		static CollectionSettingData()
		{
			// 加载配置文件
			Setting = AssetDatabase.LoadAssetAtPath<CollectionSetting>(EditorDefine.CollectorSettingFilePath);
			if (Setting == null)
			{
				Debug.LogWarning($"Create new CollectionSetting.asset : {EditorDefine.CollectorSettingFilePath}");
				Setting = ScriptableObject.CreateInstance<CollectionSetting>();
				EditorTools.CreateFileDirectory(EditorDefine.CollectorSettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.CollectorSettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log("Load CollectionSetting.asset ok");
			}
		}

		/// <summary>
		/// 存储文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
			}
		}

		/// <summary>
		/// 添加元素
		/// </summary>
		public static void AddElement(string folderPath)
		{
			if (IsContainsElement(folderPath) == false)
			{
				CollectionSetting.Wrapper element = new CollectionSetting.Wrapper();
				element.FolderPath = folderPath;
				Setting.Elements.Add(element);
				SaveFile();
			}
		}

		/// <summary>
		/// 移除元素
		/// </summary>
		public static void RemoveElement(string folderPath)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].FolderPath == folderPath)
				{
					Setting.Elements.RemoveAt(i);
					break;
				}
			}
			SaveFile();
		}

		/// <summary>
		/// 编辑元素
		/// </summary>
		public static void ModifyElement(string folderPath, CollectionSetting.EFolderPackRule packRule, CollectionSetting.EBundleLabelRule labelRule)
		{
			// 注意：这里强制修改忽略文件夹的命名规则为None
			if (packRule == CollectionSetting.EFolderPackRule.Ignore)
			{
				labelRule = CollectionSetting.EBundleLabelRule.None;
			}
			else
			{
				if (labelRule == CollectionSetting.EBundleLabelRule.None)
					labelRule = CollectionSetting.EBundleLabelRule.LabelByFilePath;
			}

			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].FolderPath == folderPath)
				{
					Setting.Elements[i].PackRule = packRule;
					Setting.Elements[i].LabelRule = labelRule;
					break;
				}
			}
			SaveFile();
		}

		/// <summary>
		/// 是否包含元素
		/// </summary>
		public static bool IsContainsElement(string folderPath)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].FolderPath == folderPath)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 获取所有的打包路径
		/// </summary>
		public static List<string> GetAllCollectPath()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				CollectionSetting.Wrapper wrapper = Setting.Elements[i];
				if (wrapper.PackRule == CollectionSetting.EFolderPackRule.Collect)
					result.Add(wrapper.FolderPath);
			}
			return result;
		}

		/// <summary>
		/// 是否收集该资源
		/// </summary>
		public static bool IsCollectAsset(string assetPath)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				CollectionSetting.Wrapper wrapper = Setting.Elements[i];
				if (wrapper.PackRule == CollectionSetting.EFolderPackRule.Collect)
				{
					if (assetPath.StartsWith(wrapper.FolderPath))
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 是否忽略该资源
		/// </summary>
		public static bool IsIgnoreAsset(string assetPath)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				CollectionSetting.Wrapper wrapper = Setting.Elements[i];
				if (wrapper.PackRule == CollectionSetting.EFolderPackRule.Ignore)
				{
					if (assetPath.StartsWith(wrapper.FolderPath))
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 获取资源的打包标签
		/// </summary>
		public static string GetAssetBundleLabel(string assetPath)
		{
			// 注意：一个资源有可能被多个规则覆盖
			List<CollectionSetting.Wrapper> filterWrappers = new List<CollectionSetting.Wrapper>();
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				CollectionSetting.Wrapper wrapper = Setting.Elements[i];
				if (assetPath.StartsWith(wrapper.FolderPath))
				{
					filterWrappers.Add(wrapper);
				}
			}

			// 我们使用路径最深层的规则
			CollectionSetting.Wrapper findWrapper = null;
			for (int i = 0; i < filterWrappers.Count; i++)
			{
				CollectionSetting.Wrapper wrapper = filterWrappers[i];
				if (findWrapper == null)
				{
					findWrapper = wrapper;
					continue;
				}
				if (wrapper.FolderPath.Length > findWrapper.FolderPath.Length)
					findWrapper = wrapper;
			}

			// 如果没有找到命名规则
			if (findWrapper == null)
			{
				return assetPath.Remove(assetPath.LastIndexOf("."));
			}

			// 根据规则设置获取标签名称
			if (findWrapper.LabelRule == CollectionSetting.EBundleLabelRule.None)
			{
				// 注意：如果依赖资源来自于忽略文件夹，那么会触发这个异常
				throw new Exception($"CollectionSetting has depend asset in ignore folder : {findWrapper.FolderPath}");
			}
			else if (findWrapper.LabelRule == CollectionSetting.EBundleLabelRule.LabelByFileName)
			{
				return Path.GetFileNameWithoutExtension(assetPath); // "C:\Demo\Assets\Config\test.txt" --> "test"
			}
			else if (findWrapper.LabelRule == CollectionSetting.EBundleLabelRule.LabelByFilePath)
			{
				return assetPath.Remove(assetPath.LastIndexOf(".")); // "C:\Demo\Assets\Config\test.txt" --> "C:\Demo\Assets\Config\test"
			}
			else if (findWrapper.LabelRule == CollectionSetting.EBundleLabelRule.LabelByFolderName)
			{
				string temp = Path.GetDirectoryName(assetPath); // "C:\Demo\Assets\Config\test.txt" --> "C:\Demo\Assets\Config"
				return Path.GetFileName(temp); // "C:\Demo\Assets\Config" --> "Config"
			}
			else if (findWrapper.LabelRule == CollectionSetting.EBundleLabelRule.LabelByFolderPath)
			{
				return Path.GetDirectoryName(assetPath); // "C:\Demo\Assets\Config\test.txt" --> "C:\Demo\Assets\Config"
			}
			else
			{
				throw new NotImplementedException($"{findWrapper.LabelRule}");
			}
		}
	}
}