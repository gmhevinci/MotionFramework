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
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	public static class AssetImporterSettingData
	{
		/// <summary>
		/// 导入器类型集合
		/// </summary>
		public static readonly Dictionary<string, System.Type> CacheTypes = new Dictionary<string, System.Type>();

		/// <summary>
		/// 导入器集合
		/// </summary>
		public static readonly Dictionary<string, IAssetProcessor> CacheProcessor = new Dictionary<string, IAssetProcessor>();

		private static AssetImporterSetting _setting = null;
		public static AssetImporterSetting Setting
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
			_setting = AssetDatabase.LoadAssetAtPath<AssetImporterSetting>(EditorDefine.AssetImporterSettingFilePath);
			if (_setting == null)
			{
				Debug.LogWarning($"Create new {nameof(AssetImporterSetting)}.asset : {EditorDefine.AssetImporterSettingFilePath}");
				_setting = ScriptableObject.CreateInstance<AssetImporterSetting>();
				EditorTools.CreateFileDirectory(EditorDefine.AssetImporterSettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.AssetImporterSettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log($"Load {nameof(AssetImporterSetting)}.asset ok");
			}

			// Clear
			CacheTypes.Clear();
			CacheProcessor.Clear();

			// 获取所有资源处理器类型
			List<Type> types = AssemblyUtility.GetAssignableTypes(AssemblyUtility.MotionFrameworkAssemblyEditorName, typeof(IAssetProcessor));
			List<Type> temps = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IAssetProcessor));
			types.AddRange(temps);
			for (int i = 0; i < types.Count; i++)
			{
				Type type = types[i];
				if (CacheTypes.ContainsKey(type.Name) == false)
					CacheTypes.Add(type.Name, type);
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
				AssetImporterSetting.Wrapper element = new AssetImporterSetting.Wrapper();
				element.FolderPath = folderPath;
				element.ProcessorName = nameof(DefaultProcessor);
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
		public static void ModifyElement(string folderPath, string processorName)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].FolderPath == folderPath)
				{
					Setting.Elements[i].ProcessorName = processorName;
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
		/// 获取资源处理器
		/// </summary>
		/// <param name="importAssetPath">导入的资源路径</param>
		/// <returns>如果该资源的路径没包含在列表里返回NULL</returns>
		public static IAssetProcessor GetCustomProcessor(string importAssetPath)
		{
			// 如果是过滤文件
			string fileName = Path.GetFileNameWithoutExtension(importAssetPath);
			if (fileName.EndsWith("@"))
				return null;

			// 获取处理器类名
			string className = null;
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				var element = Setting.Elements[i];
				if (importAssetPath.Contains(element.FolderPath))
				{
					className = element.ProcessorName;
					break;
				}
			}
			if (string.IsNullOrEmpty(className))
				return null;

			// 先从缓存里获取
			IAssetProcessor processor = null;
			if (CacheProcessor.TryGetValue(className, out processor))
				return processor;

			// 如果不存在创建处理器
			System.Type type;
			if (CacheTypes.TryGetValue(className, out type))
			{
				processor = (IAssetProcessor)Activator.CreateInstance(type);
				return processor;
			}
			else
			{
				Debug.LogError($"资源处理器类型无效：{className}");
				return null;
			}
		}
	}
}