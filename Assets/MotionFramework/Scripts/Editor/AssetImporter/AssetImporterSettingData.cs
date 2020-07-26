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
		private static readonly Dictionary<string, System.Type> _cacheTypes = new Dictionary<string, System.Type>();

		/// <summary>
		/// 导入器实例集合
		/// </summary>
		private static readonly Dictionary<string, IAssetProcessor> _cacheProcessor = new Dictionary<string, IAssetProcessor>();

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

			// 清空缓存集合
			_cacheTypes.Clear();
			_cacheProcessor.Clear();

			// 获取所有资源处理器类型
			List<Type> types = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IAssetProcessor));
			types.Add(typeof(DefaultProcessor));
			for (int i = 0; i < types.Count; i++)
			{
				Type type = types[i];
				if (_cacheTypes.ContainsKey(type.Name) == false)
					_cacheTypes.Add(type.Name, type);
			}
		}

		/// <summary>
		/// 获取所有导入器名称列表
		/// </summary>
		public static List<string> GetProcessorNames()
		{
			if (_setting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cacheTypes)
			{
				names.Add(pair.Key);
			}
			return names;
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

		public static void AddElement(string directory)
		{
			if (IsContainsElement(directory) == false)
			{
				AssetImporterSetting.Wrapper element = new AssetImporterSetting.Wrapper();
				element.ProcessDirectory = directory;
				element.ProcessorName = nameof(DefaultProcessor);
				Setting.Elements.Add(element);
				SaveFile();
			}
		}
		public static void RemoveElement(string directory)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].ProcessDirectory == directory)
				{
					Setting.Elements.RemoveAt(i);
					break;
				}
			}
			SaveFile();
		}
		public static void ModifyElement(string directory, string processorName)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].ProcessDirectory == directory)
				{
					Setting.Elements[i].ProcessorName = processorName;
					break;
				}
			}
			SaveFile();
		}
		public static bool IsContainsElement(string directory)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].ProcessDirectory == directory)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 获取资源处理器实例
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
				if (importAssetPath.Contains(element.ProcessDirectory))
				{
					className = element.ProcessorName;
					break;
				}
			}
			if (string.IsNullOrEmpty(className))
				return null;

			return GetProcessorInstance(className);
		}
		private static IAssetProcessor GetProcessorInstance(string className)
		{
			if (_cacheProcessor.TryGetValue(className, out IAssetProcessor instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cacheTypes.TryGetValue(className, out Type type))
			{
				instance = (IAssetProcessor)Activator.CreateInstance(type);
				_cacheProcessor.Add(className, instance);
				return instance;
			}
			else
			{
				throw new Exception($"资源处理器类型无效：{className}");
			}
		}
	}
}