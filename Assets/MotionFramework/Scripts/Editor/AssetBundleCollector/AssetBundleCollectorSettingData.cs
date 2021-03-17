//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MotionFramework.Utility;
using MotionFramework.Patch;

namespace MotionFramework.Editor
{
	public static class AssetBundleCollectorSettingData
	{
		private static readonly Dictionary<string, System.Type> _cacheLabelTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, IBundleLabel> _cacheLabelInstance = new Dictionary<string, IBundleLabel>();

		private static readonly Dictionary<string, System.Type> _cacheFilterTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, ISearchFilter> _cacheFilterInstance = new Dictionary<string, ISearchFilter>();


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

		public static List<string> GetLabelClassNames()
		{
			if (_setting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cacheLabelTypes)
			{
				names.Add(pair.Key);
			}
			return names;
		}
		public static List<string> GetFilterClassNames()
		{
			if (_setting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cacheFilterTypes)
			{
				names.Add(pair.Key);
			}
			return names;
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

			// 收集器类型
			{
				// 清空缓存集合
				_cacheLabelTypes.Clear();
				_cacheLabelInstance.Clear();

				// 获取所有类型
				List<Type> types = new List<Type>(100)
				{
					typeof(LabelByFilePath),
					typeof(LabelByFolderPath)
				};
				var customTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IBundleLabel));
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (_cacheLabelTypes.ContainsKey(type.Name) == false)
						_cacheLabelTypes.Add(type.Name, type);
				}
			}

			// 过滤器类型
			{
				// 清空缓存集合
				_cacheFilterTypes.Clear();
				_cacheFilterInstance.Clear();

				// 获取所有类型
				List<Type> types = new List<Type>(100)
				{
					typeof(SearchAll),
					typeof(SearchScene),
					typeof(SearchPrefab),
					typeof(SearchSprite)
				};
				var customTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(ISearchFilter));
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (_cacheFilterTypes.ContainsKey(type.Name) == false)
						_cacheFilterTypes.Add(type.Name, type);
				}
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

		// 收集器相关
		public static void AddCollector(string directory)
		{
			// 末尾添加路径分隔符号
			if (directory.EndsWith("/") == false)
				directory = $"{directory}/";

			// 检测收集器路径冲突
			if (CheckConflict(directory))
				return;

			AssetBundleCollectorSetting.Collector element = new AssetBundleCollectorSetting.Collector();
			element.CollectDirectory = directory;
			element.LabelClassName = nameof(LabelByFilePath);
			element.FilterClassName = nameof(SearchAll);
			Setting.Collectors.Add(element);
			SaveFile();
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
		public static void ModifyCollector(string directory, string labelClassName, string filterClassName)
		{
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				if (Setting.Collectors[i].CollectDirectory == directory)
				{
					Setting.Collectors[i].LabelClassName = labelClassName;
					Setting.Collectors[i].FilterClassName = filterClassName;
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
		public static bool CheckConflict(string directory)
		{
			if (IsContainsCollector(directory))
			{
				Debug.LogError($"Asset collector already existed : {directory}");
				return true;
			}

			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				var wrapper = Setting.Collectors[i];
				string compareDirectory = wrapper.CollectDirectory;
				if (directory.StartsWith(compareDirectory))
				{
					Debug.LogError($"New asset collector \"{directory}\" conflict with \"{compareDirectory}\"");
					return true;
				}
				if (compareDirectory.StartsWith(directory))
				{
					Debug.LogError($"New asset collector {directory} conflict with {compareDirectory}");
					return true;
				}
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
				result.Add(wrapper.CollectDirectory);
			}
			return result;
		}

		/// <summary>
		/// 获取收集器数量
		/// </summary>
		public static int GetCollecterCount()
		{
			return Setting.Collectors.Count;
		}

		/// <summary>
		/// 获取所有收集的资源
		/// </summary>
		/// <returns>返回资源路径列表</returns>
		public static List<string> GetAllCollectAssets()
		{
			List<string> result = new List<string>(10000);
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector collector = Setting.Collectors[i];
				string collectDirectory = collector.CollectDirectory.TrimEnd('/'); //注意：AssetDatabase不支持末尾带分隔符的文件夹路径
				string[] guids = AssetDatabase.FindAssets(string.Empty, new string[] { collectDirectory });
				foreach (string guid in guids)
				{
					string assetPath = AssetDatabase.GUIDToAssetPath(guid);
					if (ValidateAsset(assetPath) == false)
						continue;
					if (FilterAsset(assetPath, collector.FilterClassName) == false)
						continue;
					if (result.Contains(assetPath) == false)
						result.Add(assetPath);
				}
			}
			return result;
		}
		private static bool FilterAsset(string assetPath, string filterClassName)
		{
			// 根据规则设置获取标签名称
			ISearchFilter filter = GetSearchFilterInstance(filterClassName);
			return filter.FilterAsset(assetPath);
		}

		/// <summary>
		/// 检测资源是否有效
		/// </summary>
		public static bool ValidateAsset(string assetPath)
		{
			if (!assetPath.StartsWith("Assets/"))
				return false;

			if (AssetDatabase.IsValidFolder(assetPath))
				return false;

			// 注意：忽略编辑器下的类型资源
			Type type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
			if (type == typeof(LightingDataAsset))
				return false;

			string ext = System.IO.Path.GetExtension(assetPath);
			if (ext == "" || ext == ".dll" || ext == ".cs" || ext == ".js" || ext == ".boo" || ext == ".meta")
				return false;

			return true;
		}

		/// <summary>
		/// 是否收集该资源
		/// </summary>
		public static bool IsCollectAsset(string assetPath)
		{
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = Setting.Collectors[i];
				if (assetPath.StartsWith(wrapper.CollectDirectory))
					return true;
			}
			return false;
		}

		/// <summary>
		/// 获取资源的打包标签和变种
		/// </summary>
		public struct BundleLabelAndVariant
		{
			public string BundleLabel;
			public string BundleVariant;
		}
		public static BundleLabelAndVariant GetBundleLabelAndVariant(string assetPath)
		{
			string label;

			// 获取收集器
			AssetBundleCollectorSetting.Collector findWrapper = null;
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = Setting.Collectors[i];
				if (assetPath.StartsWith(wrapper.CollectDirectory))
				{
					findWrapper = wrapper;
					break;
				}
			}

			// 如果没有找到收集器
			if (findWrapper == null)
			{
				IBundleLabel defaultLabel = new LabelByFilePath();
				label = defaultLabel.GetAssetBundleLabel(assetPath);
			}
			else
			{
				// 根据规则设置获取标签名称
				IBundleLabel bundleLabel = GetCollectorInstance(findWrapper.LabelClassName);
				label = bundleLabel.GetAssetBundleLabel(assetPath);
			}

			// 注意：如果资源所在文件夹的名称包含后缀符号，则为变体资源
			string folderName = Path.GetDirectoryName(assetPath); // "Assets/Texture.HD/background.jpg" --> "Assets/Texture.HD"
			if (Path.HasExtension(folderName))
			{
				string extension = Path.GetExtension(folderName);
				BundleLabelAndVariant result = new BundleLabelAndVariant();
				result.BundleLabel = EditorTools.GetRegularPath(label.Replace(extension, string.Empty));
				result.BundleVariant = extension.RemoveFirstChar();
				return result;
			}
			else
			{
				BundleLabelAndVariant result = new BundleLabelAndVariant();
				result.BundleLabel = EditorTools.GetRegularPath(label);
				result.BundleVariant = PatchDefine.AssetBundleDefaultVariant;
				return result;
			}
		}

		/// <summary>
		/// 获取所有DLC文件列表
		/// </summary>
		public static string[] GetDLCFiles()
		{
			return Setting.DLCFiles.ToArray();
		}

		private static IBundleLabel GetCollectorInstance(string className)
		{
			if (_cacheLabelInstance.TryGetValue(className, out IBundleLabel instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cacheLabelTypes.TryGetValue(className, out Type type))
			{
				instance = (IBundleLabel)Activator.CreateInstance(type);
				_cacheLabelInstance.Add(className, instance);
				return instance;
			}
			else
			{
				throw new Exception($"资源收集器类型无效：{className}");
			}
		}
		private static ISearchFilter GetSearchFilterInstance(string className)
		{
			if (_cacheFilterInstance.TryGetValue(className, out ISearchFilter instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cacheFilterTypes.TryGetValue(className, out Type type))
			{
				instance = (ISearchFilter)Activator.CreateInstance(type);
				_cacheFilterInstance.Add(className, instance);
				return instance;
			}
			else
			{
				throw new Exception($"资源过滤器类型无效：{className}");
			}
		}
	}
}