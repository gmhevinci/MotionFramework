//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	public static class AssetScanerSettingData
	{
		/// <summary>
		/// 类型集合
		/// </summary>
		private static readonly Dictionary<string, System.Type> _cacheTypes = new Dictionary<string, System.Type>();

		private static AssetScanerSetting _setting = null;
		public static AssetScanerSetting Setting
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
			_setting = AssetDatabase.LoadAssetAtPath<AssetScanerSetting>(EditorDefine.AssetScanerSettingFilePath);
			if (_setting == null)
			{
				_setting = ScriptableObject.CreateInstance<AssetScanerSetting>();
				EditorTools.CreateFileDirectory(EditorDefine.AssetScanerSettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.AssetScanerSettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log($"Load {nameof(AssetScanerSetting)}.asset ok");
			}

			// 清空缓存集合
			_cacheTypes.Clear();

			// 获取所有类型
			List<Type> types = new List<Type>(100)
			{
				typeof(DefaultScaner)
			};
			var customTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IAssetScaner));
			types.AddRange(customTypes);
			for (int i = 0; i < types.Count; i++)
			{
				Type type = types[i];
				if (_cacheTypes.ContainsKey(type.Name) == false)
					_cacheTypes.Add(type.Name, type);
			}
		}

		/// <summary>
		/// 获取所有扫描器名称列表
		/// </summary>
		public static List<string> GetScanerNames()
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
				AssetScanerSetting.Wrapper element = new AssetScanerSetting.Wrapper();
				element.ScanerDirectory = directory;
				element.ScanerName = nameof(DefaultScaner);
				Setting.Elements.Add(element);
				SaveFile();
			}
		}
		public static void RemoveElement(string directory)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].ScanerDirectory == directory)
				{
					Setting.Elements.RemoveAt(i);
					break;
				}
			}
			SaveFile();
		}
		public static void ModifyElement(string directory, string scanerName)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].ScanerDirectory == directory)
				{
					Setting.Elements[i].ScanerName = scanerName;
					break;
				}
			}
			SaveFile();
		}
		public static bool IsContainsElement(string directory)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].ScanerDirectory == directory)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 扫描所有资源
		/// </summary>
		public static Dictionary<string, List<ScanReport>> ScanAllAssets()
		{
			Dictionary<string, List<ScanReport>> result = new Dictionary<string, List<ScanReport>>();
			int progressBarCount = 0;
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				var element = Setting.Elements[i];
				string directory = element.ScanerDirectory;
				string className = element.ScanerName;

				IAssetScaner scaner = CreateScanerInstance(className);
				List<ScanReport> reports = scaner.Scan(directory);
				result.Add(directory, reports);

				EditorTools.DisplayProgressBar("资源扫描", ++progressBarCount, Setting.Elements.Count);
			}
			EditorTools.ClearProgressBar();
			return result;
		}
		private static IAssetScaner CreateScanerInstance(string className)
		{
			if (_cacheTypes.TryGetValue(className, out Type type))
			{
				return (IAssetScaner)Activator.CreateInstance(type);
			}
			else
			{
				throw new Exception($"资源处理器类型无效：{className}");
			}
		}
	}
}