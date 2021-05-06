//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MotionFramework.Utility;
using MotionFramework.Patch;

namespace MotionFramework.Editor
{
	public static class AssetBundleCollectorSettingData
	{
		public struct BundleLabelAndVariant
		{
			public string BundleLabel { private set; get; }
			public string BundleVariant { private set; get; }

			public BundleLabelAndVariant(string label, string variant)
			{
				BundleLabel = EditorTools.GetRegularPath(label);
				BundleVariant = variant;
			}
		}

		private static readonly Dictionary<string, System.Type> _cachePackRuleTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, IPackRule> _cachePackRuleInstance = new Dictionary<string, IPackRule>();

		private static readonly Dictionary<string, System.Type> _cacheFilterRuleTypes = new Dictionary<string, System.Type>();
		private static readonly Dictionary<string, IFilterRule> _cacheFilterRuleInstance = new Dictionary<string, IFilterRule>();


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

		public static List<string> GetPackRuleClassNames()
		{
			if (_setting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cachePackRuleTypes)
			{
				names.Add(pair.Key);
			}
			return names;
		}
		public static List<string> GetFilterRuleClassNames()
		{
			if (_setting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cacheFilterRuleTypes)
			{
				names.Add(pair.Key);
			}
			return names;
		}
		public static bool HasPackRuleClassName(string className)
		{
			foreach (var pair in _cachePackRuleTypes)
			{
				if (pair.Key == className)
					return true;
			}
			return false;
		}
		public static bool HasFilterRuleClassName(string className)
		{
			foreach (var pair in _cacheFilterRuleTypes)
			{
				if (pair.Key == className)
					return true;
			}
			return false;
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

			// IPackRule
			{
				// 清空缓存集合
				_cachePackRuleTypes.Clear();
				_cachePackRuleInstance.Clear();

				// 获取所有类型
				List<Type> types = new List<Type>(100)
				{
					typeof(PackExplicit),
					typeof(PackDirectory)
				};
				var customTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IPackRule));
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (_cachePackRuleTypes.ContainsKey(type.Name) == false)
						_cachePackRuleTypes.Add(type.Name, type);
				}
			}

			// IFilterRule
			{
				// 清空缓存集合
				_cacheFilterRuleTypes.Clear();
				_cacheFilterRuleInstance.Clear();

				// 获取所有类型
				List<Type> types = new List<Type>(100)
				{
					typeof(CollectAll),
					typeof(CollectScene),
					typeof(CollectPrefab),
					typeof(CollectSprite)
				};
				var customTypes = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IFilterRule));
				types.AddRange(customTypes);
				for (int i = 0; i < types.Count; i++)
				{
					Type type = types[i];
					if (_cacheFilterRuleTypes.ContainsKey(type.Name) == false)
						_cacheFilterRuleTypes.Add(type.Name, type);
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

		// 着色器相关
		public static void ModifyShader(bool isCollectAllShaders, string shadersBundleName)
		{
			if (string.IsNullOrEmpty(shadersBundleName))
				return;
			Setting.IsCollectAllShaders = isCollectAllShaders;
			Setting.ShadersBundleName = shadersBundleName;
			SaveFile();
		}

		// 收集器相关
		public static void ClearAllCollector()
		{
			Setting.Collectors.Clear();
			SaveFile();
		}
		public static void AddCollector(string directory, string packRuleClassName, string filterRuleClassName, bool dontWriteAssetPath, bool saveFile = true)
		{
			// 末尾添加路径分隔符号
			if (directory.EndsWith("/") == false)
				directory = $"{directory}/";

			// 检测收集器路径冲突
			if (CheckConflict(directory))
				return;

			AssetBundleCollectorSetting.Collector element = new AssetBundleCollectorSetting.Collector();
			element.CollectDirectory = directory;
			element.PackRuleClassName = packRuleClassName;
			element.FilterRuleClassName = filterRuleClassName;
			element.DontWriteAssetPath = dontWriteAssetPath;
			Setting.Collectors.Add(element);

			if (saveFile)
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
		public static void ModifyCollector(string directory, string packRuleClassName, string filterRuleClassName, bool dontWriteAssetPath)
		{
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				if (Setting.Collectors[i].CollectDirectory == directory)
				{
					Setting.Collectors[i].PackRuleClassName = packRuleClassName;
					Setting.Collectors[i].FilterRuleClassName = filterRuleClassName;
					Setting.Collectors[i].DontWriteAssetPath = dontWriteAssetPath;
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
		/// 获取收集器总数
		/// </summary>
		public static int GetCollecterCount()
		{
			return Setting.Collectors.Count;
		}

		/// <summary>
		/// 获取所有的收集路径
		/// </summary>
		public static List<string> GetAllCollectDirectory()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = Setting.Collectors[i];
				result.Add(wrapper.CollectDirectoryTrimEndSeparator);
			}
			return result;
		}

		/// <summary>
		/// 获取所有收集的资源
		/// </summary>
		/// <returns>返回资源路径列表</returns>
		public static List<AssetCollectInfo> GetAllCollectAssets()
		{
			Dictionary<string, AssetCollectInfo> result = new Dictionary<string, AssetCollectInfo>(10000);
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector collector = Setting.Collectors[i];

				// 获取收集目录下的所有资源对象的GUID包括子文件夹
				string collectDirectory = collector.CollectDirectoryTrimEndSeparator;
				string[] guids = AssetDatabase.FindAssets(string.Empty, new string[] { collectDirectory });
				foreach (string guid in guids)
				{
					string assetPath = AssetDatabase.GUIDToAssetPath(guid);
					if (IsValidateAsset(assetPath) == false)
						continue;
					if (IsCollectAsset(assetPath, collector.FilterRuleClassName) == false)
						continue;
					if (result.ContainsKey(assetPath) == false)
						result.Add(assetPath, new AssetCollectInfo(assetPath, collector.DontWriteAssetPath));
				}
			}
			return result.Values.ToList();
		}
		private static bool IsCollectAsset(string assetPath, string filterRuleClassName)
		{
			if (Setting.IsCollectAllShaders)
			{
				Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
					return true;
			}

			// 根据规则设置获取标签名称
			IFilterRule filterRuleInstance = GetFilterRuleInstance(filterRuleClassName);
			return filterRuleInstance.IsCollectAsset(assetPath);
		}

		/// <summary>
		/// 检测资源是否有效
		/// </summary>
		public static bool IsValidateAsset(string assetPath)
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
		/// 获取资源的打包信息
		/// </summary>
		public static BundleLabelAndVariant GetBundleLabelAndVariant(string assetPath)
		{
			// 如果收集全路径着色器		
			if (Setting.IsCollectAllShaders)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
				if (assetType == typeof(UnityEngine.Shader))
				{
					return new BundleLabelAndVariant(Setting.ShadersBundleName, PatchDefine.AssetBundleDefaultVariant);
				}
			}

			// 获取收集器
			AssetBundleCollectorSetting.Collector findCollector = null;
			for (int i = 0; i < Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector collector = Setting.Collectors[i];
				if (assetPath.StartsWith(collector.CollectDirectory))
				{
					findCollector = collector;
					break;
				}
			}

			string bundleLabel;

			// 如果没有找到收集器
			if (findCollector == null)
			{
				IPackRule defaultInstance = new PackExplicit();
				bundleLabel = defaultInstance.GetAssetBundleLabel(assetPath);
			}
			else
			{
				// 根据规则设置获取标签名称
				IPackRule getInstance = GetPackRuleInstance(findCollector.PackRuleClassName);
				bundleLabel = getInstance.GetAssetBundleLabel(assetPath);
			}

			// 注意：如果资源所在文件夹的名称包含后缀符号，则为变体资源
			string assetDirectory = Path.GetDirectoryName(assetPath); // "Assets/Texture.HD/background.jpg" --> "Assets/Texture.HD"
			if (Path.HasExtension(assetDirectory))
			{
				string extension = Path.GetExtension(assetDirectory);
				bundleLabel = bundleLabel.Replace(extension, string.Empty);
				string bundleVariant = extension.RemoveFirstChar();
				return new BundleLabelAndVariant(bundleLabel, bundleVariant);
			}
			else
			{
				return new BundleLabelAndVariant(bundleLabel, PatchDefine.AssetBundleDefaultVariant);
			}
		}

		/// <summary>
		/// 获取所有DLC文件列表
		/// </summary>
		public static string[] GetDLCFiles()
		{
			return Setting.DLCFiles.ToArray();
		}

		private static IPackRule GetPackRuleInstance(string className)
		{
			if (_cachePackRuleInstance.TryGetValue(className, out IPackRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cachePackRuleTypes.TryGetValue(className, out Type type))
			{
				instance = (IPackRule)Activator.CreateInstance(type);
				_cachePackRuleInstance.Add(className, instance);
				return instance;
			}
			else
			{
				throw new Exception($"{nameof(IPackRule)}类型无效：{className}");
			}
		}
		private static IFilterRule GetFilterRuleInstance(string className)
		{
			if (_cacheFilterRuleInstance.TryGetValue(className, out IFilterRule instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cacheFilterRuleTypes.TryGetValue(className, out Type type))
			{
				instance = (IFilterRule)Activator.CreateInstance(type);
				_cacheFilterRuleInstance.Add(className, instance);
				return instance;
			}
			else
			{
				throw new Exception($"{nameof(IFilterRule)}类型无效：{className}");
			}
		}
	}
}