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

		/// <summary>
		/// 添加元素
		/// </summary>
		public static void AddElement(string directory)
		{
			if (IsContainsElement(directory) == false)
			{
				AssetBundleCollectorSetting.Wrapper element = new AssetBundleCollectorSetting.Wrapper();
				element.CollectDirectory = directory;
				Setting.Elements.Add(element);
				SaveFile();
			}
		}

		/// <summary>
		/// 移除元素
		/// </summary>
		public static void RemoveElement(string directory)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].CollectDirectory == directory)
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
		public static void ModifyElement(string directory, AssetBundleCollectorSetting.ECollectRule collectRule, string collectorName)
		{
			// 注意：这里强制修改忽略文件夹的命名规则为None
			if (collectRule == AssetBundleCollectorSetting.ECollectRule.Ignore)
			{
				collectorName = nameof(LabelNone);
			}
			else
			{
				if (collectorName == nameof(LabelNone))
					collectorName = nameof(LabelByFilePath);
			}

			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].CollectDirectory == directory)
				{
					Setting.Elements[i].CollectRule = collectRule;
					Setting.Elements[i].CollectorName = collectorName;
					break;
				}
			}
			SaveFile();
		}

		/// <summary>
		/// 是否包含元素
		/// </summary>
		public static bool IsContainsElement(string directory)
		{
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				if (Setting.Elements[i].CollectDirectory == directory)
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
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				AssetBundleCollectorSetting.Wrapper wrapper = Setting.Elements[i];
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
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				AssetBundleCollectorSetting.Wrapper wrapper = Setting.Elements[i];
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
			List<AssetBundleCollectorSetting.Wrapper> filterWrappers = new List<AssetBundleCollectorSetting.Wrapper>();
			for (int i = 0; i < Setting.Elements.Count; i++)
			{
				AssetBundleCollectorSetting.Wrapper wrapper = Setting.Elements[i];
				if (assetPath.StartsWith(wrapper.CollectDirectory))
				{
					filterWrappers.Add(wrapper);
				}
			}

			// 我们使用路径最深层的收集器
			AssetBundleCollectorSetting.Wrapper findWrapper = null;
			for (int i = 0; i < filterWrappers.Count; i++)
			{
				AssetBundleCollectorSetting.Wrapper wrapper = filterWrappers[i];
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
	}
}