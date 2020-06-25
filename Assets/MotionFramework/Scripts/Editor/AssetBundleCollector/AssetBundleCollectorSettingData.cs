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
	public static class AssetBundleCollectorSettingData
	{
		private static AssetBundleCollectorSetting _setting = null;
		public static AssetBundleCollectorSetting Setting
		{
			get
			{
				if (_setting == null)
					LoadSettingData();
				return _setting;
			}
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			// 加载配置文件
			_setting = AssetDatabase.LoadAssetAtPath<AssetBundleCollectorSetting>(EditorDefine.AssetBundleCollectorSettingFilePath);
			if (_setting == null)
			{
				Debug.LogWarning($"Create new {nameof(AssetBundleCollectorSetting)}.asset : {EditorDefine.AssetBundleCollectorSettingFilePath}");
				_setting = ScriptableObject.CreateInstance<AssetBundleCollectorSetting>();
				EditorTools.CreateFileDirectory(EditorDefine.AssetBundleCollectorSettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.AssetBundleCollectorSettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log($"Load {nameof(AssetBundleCollectorSetting)}.asset ok");
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
				AssetBundleCollectorSetting.Wrapper element = new AssetBundleCollectorSetting.Wrapper();
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
		public static void ModifyElement(string folderPath, AssetBundleCollectorSetting.EFolderPackRule packRule, AssetBundleCollectorSetting.EBundleLabelRule labelRule)
		{
			// 注意：这里强制修改忽略文件夹的命名规则为None
			if (packRule == AssetBundleCollectorSetting.EFolderPackRule.Ignore)
			{
				labelRule = AssetBundleCollectorSetting.EBundleLabelRule.None;
			}
			else
			{
				if (labelRule == AssetBundleCollectorSetting.EBundleLabelRule.None)
					labelRule = AssetBundleCollectorSetting.EBundleLabelRule.LabelByFilePath;
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
				AssetBundleCollectorSetting.Wrapper wrapper = Setting.Elements[i];
				if (wrapper.PackRule == AssetBundleCollectorSetting.EFolderPackRule.Collect)
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
				AssetBundleCollectorSetting.Wrapper wrapper = Setting.Elements[i];
				if (wrapper.PackRule == AssetBundleCollectorSetting.EFolderPackRule.Collect)
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
				AssetBundleCollectorSetting.Wrapper wrapper = Setting.Elements[i];
				if (wrapper.PackRule == AssetBundleCollectorSetting.EFolderPackRule.Ignore)
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
			List<AssetBundleCollectorSetting.Wrapper> filterWrappers = new List<AssetBundleCollectorSetting.Wrapper>();
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				AssetBundleCollectorSetting.Wrapper wrapper = Setting.Elements[i];
				if (assetPath.StartsWith(wrapper.FolderPath))
				{
					filterWrappers.Add(wrapper);
				}
			}

			// 我们使用路径最深层的规则
			AssetBundleCollectorSetting.Wrapper findWrapper = null;
			for (int i = 0; i < filterWrappers.Count; i++)
			{
				AssetBundleCollectorSetting.Wrapper wrapper = filterWrappers[i];
				if (findWrapper == null)
				{
					findWrapper = wrapper;
					continue;
				}
				if (wrapper.FolderPath.Length > findWrapper.FolderPath.Length)
					findWrapper = wrapper;
			}

			// 如果没有找到命名规则，文件路径作为默认的标签名
			if (findWrapper == null)
			{
				return assetPath.Remove(assetPath.LastIndexOf(".")); // "Assets/Config/test.txt" --> "Assets/Config/test"
			}

			// 根据规则设置获取标签名称
			if (findWrapper.LabelRule == AssetBundleCollectorSetting.EBundleLabelRule.None)
			{
				// 注意：如果依赖资源来自于忽略文件夹，那么会触发这个异常
				throw new Exception($"CollectionSetting has depend asset in ignore folder : {findWrapper.FolderPath}");
			}
			else if (findWrapper.LabelRule == AssetBundleCollectorSetting.EBundleLabelRule.LabelByFilePath)
			{
				return assetPath.Remove(assetPath.LastIndexOf(".")); // "Assets/Config/test.txt" --> "Assets/Config/test"
			}
			else if (findWrapper.LabelRule == AssetBundleCollectorSetting.EBundleLabelRule.LabelByFolderPath)
			{
				return Path.GetDirectoryName(assetPath); // "Assets/Config/test.txt" --> "Assets/Config"
			}
			else
			{
				throw new NotImplementedException($"{findWrapper.LabelRule}");
			}
		}
	}
}