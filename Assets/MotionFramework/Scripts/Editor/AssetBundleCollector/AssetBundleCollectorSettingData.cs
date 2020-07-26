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
	public static class AssetBundleCollectorSettingData
	{
		/// <summary>
		/// 收集器类型集合
		/// </summary>
		private static readonly Dictionary<string, System.Type> _cacheTypes = new Dictionary<string, System.Type>();

		/// <summary>
		/// 收集器实例集合
		/// </summary>
		private static readonly Dictionary<string, IAssetCollector> _cacheCollector = new Dictionary<string, IAssetCollector>();

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

			// 清空缓存集合
			_cacheTypes.Clear();
			_cacheCollector.Clear();

			// 获取所有资源收集器类型
			List<Type> types = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IAssetCollector));
			types.Add(typeof(LabelNone));
			types.Add(typeof(LabelByFilePath));
			types.Add(typeof(LabelByFolderPath));
			for (int i = 0; i < types.Count; i++)
			{
				Type type = types[i];
				if (_cacheTypes.ContainsKey(type.Name) == false)
					_cacheTypes.Add(type.Name, type);
			}
		}

		/// <summary>
		/// 获取所有收集器名称列表
		/// </summary>
		public static List<string> GetCollectorNames()
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

		// 收集器相关
		public static void AddCollector(string directory)
		{
			if (IsContainsCollector(directory) == false)
			{
				AssetBundleCollectorSetting.Collector element = new AssetBundleCollectorSetting.Collector();
				element.CollectDirectory = directory;
				Setting.Collectors.Add(element);
				SaveFile();
			}
		}
		public static void RemoveCollector(string directory)
		{
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				if (Setting.Collectors[i].CollectDirectory == directory)
				{
					Setting.Collectors.RemoveAt(i);
					break;
				}
			}
			SaveFile();
		}
		public static void ModifyCollector(string directory, AssetBundleCollectorSetting.ECollectRule collectRule, string collectorName)
		{
			// 注意：这里强制修改忽略文件夹的命名规则为None
			if (collectRule == AssetBundleCollectorSetting.ECollectRule.Ignore)
				collectorName = nameof(LabelNone);

			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				if (Setting.Collectors[i].CollectDirectory == directory)
				{
					Setting.Collectors[i].CollectRule = collectRule;
					Setting.Collectors[i].CollectorName = collectorName;
					break;
				}
			}
			SaveFile();
		}
		public static bool IsContainsCollector(string directory)
		{
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				if (Setting.Collectors[i].CollectDirectory == directory)
					return true;
			}
			return false;
		}

		// DLC相关
		public static void AddDLC(string filePath)
		{
			if (IsContainsDLC(filePath) == false)
			{
				Setting.DLCFiles.Add(filePath);
				SaveFile();
			}
		}
		public static void RemoveDLC(string filePath)
		{
			for (int i = 0; i < Setting.DLCFiles.Count; i++)
			{
				if (Setting.DLCFiles[i] == filePath)
				{
					Setting.DLCFiles.RemoveAt(i);
					break;
				}
			}
			SaveFile();
		}
		public static bool IsContainsDLC(string filePath)
		{
			for (int i = 0; i < Setting.DLCFiles.Count; i++)
			{
				if (Setting.DLCFiles[i] == filePath)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 获取所有的打包路径
		/// </summary>
		public static List<string> GetAllCollectDirectory()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = Setting.Collectors[i];
				if (wrapper.CollectRule == AssetBundleCollectorSetting.ECollectRule.Collect)
					result.Add(wrapper.CollectDirectory);
			}

			return result;
		}

		/// <summary>
		/// 是否收集该资源
		/// </summary>
		public static bool IsCollectAsset(string assetPath)
		{
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = Setting.Collectors[i];
				if (wrapper.CollectRule == AssetBundleCollectorSetting.ECollectRule.Collect)
				{
					if (assetPath.StartsWith(wrapper.CollectDirectory))
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
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = Setting.Collectors[i];
				if (wrapper.CollectRule == AssetBundleCollectorSetting.ECollectRule.Ignore)
				{
					if (assetPath.StartsWith(wrapper.CollectDirectory))
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
			// 注意：一个资源有可能被多个收集器覆盖
			List<AssetBundleCollectorSetting.Collector> filterWrappers = new List<AssetBundleCollectorSetting.Collector>();
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = Setting.Collectors[i];
				if (assetPath.StartsWith(wrapper.CollectDirectory))
				{
					filterWrappers.Add(wrapper);
				}
			}

			// 我们使用路径最深层的收集器
			AssetBundleCollectorSetting.Collector findWrapper = null;
			for (int i = 0; i < filterWrappers.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = filterWrappers[i];
				if (findWrapper == null)
				{
					findWrapper = wrapper;
					continue;
				}
				if (wrapper.CollectDirectory.Length > findWrapper.CollectDirectory.Length)
					findWrapper = wrapper;
			}

			// 如果没有找到收集器
			if (findWrapper == null)
			{
				IAssetCollector defaultCollector = new LabelByFilePath();
				return defaultCollector.GetAssetBundleLabel(assetPath);
			}

			// 根据规则设置获取标签名称
			IAssetCollector collector = GetCollectorInstance(findWrapper.CollectorName);
			return collector.GetAssetBundleLabel(assetPath);
		}
		private static IAssetCollector GetCollectorInstance(string className)
		{
			if (_cacheCollector.TryGetValue(className, out IAssetCollector instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cacheTypes.TryGetValue(className, out Type type))
			{
				instance = (IAssetCollector)Activator.CreateInstance(type);
				_cacheCollector.Add(className, instance);
				return instance;
			}
			else
			{
				throw new Exception($"资源收集器类型无效：{className}");
			}
		}

		/// <summary>
		/// 获取所有DLC文件列表
		/// </summary>
		/// <returns></returns>
		public static string[] GetDLCFiles()
		{
			return Setting.DLCFiles.ToArray();
		}
	}
}