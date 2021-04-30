//--------------------------------------------------
// Motion Framework
// Copyright©2019-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class AssetBundleCollectorWindow : EditorWindow
	{
		static AssetBundleCollectorWindow _thisInstance;

		[MenuItem("MotionTools/AssetBundle Collector", false, 101)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(AssetBundleCollectorWindow), false, "资源包收集工具", true) as AssetBundleCollectorWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		/// <summary>
		/// 上次打开的文件夹路径
		/// </summary>
		private string _lastOpenFolderPath = "Assets/";

		private string[] _packRuleClassArray = null;
		private string[] _filterRuleClassArray = null;
		private Vector2 _scrollPos = Vector2.zero;

		// 初始化相关
		private bool _isInit = false;
		private void Init()
		{
			List<string> packRuleClassNames = AssetBundleCollectorSettingData.GetPackRuleClassNames();
			_packRuleClassArray = packRuleClassNames.ToArray();

			List<string> filterRuleClassNames = AssetBundleCollectorSettingData.GetFilterRuleClassNames();
			_filterRuleClassArray = filterRuleClassNames.ToArray();
		}
		private int PackRuleClassNameToIndex(string name)
		{
			for (int i = 0; i < _packRuleClassArray.Length; i++)
			{
				if (_packRuleClassArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToPackRuleClassName(int index)
		{
			for (int i = 0; i < _packRuleClassArray.Length; i++)
			{
				if (i == index)
					return _packRuleClassArray[i];
			}
			return string.Empty;
		}
		private int FilterRuleClassNameToIndex(string name)
		{
			for (int i = 0; i < _filterRuleClassArray.Length; i++)
			{
				if (_filterRuleClassArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToFilterRuleClassName(int index)
		{
			for (int i = 0; i < _filterRuleClassArray.Length; i++)
			{
				if (i == index)
					return _filterRuleClassArray[i];
			}
			return string.Empty;
		}

		private void OnGUI()
		{
			if (_isInit == false)
			{
				_isInit = true;
				Init();
			}

			OnDrawCollector();
			OnDrawDLC();
		}
		private void OnDrawCollector()
		{
			// 着色器选项
			EditorGUILayout.Space();
			bool isCollectAllShader = EditorGUILayout.Toggle("收集所有着色器", AssetBundleCollectorSettingData.Setting.IsCollectAllShaders);
			if (isCollectAllShader != AssetBundleCollectorSettingData.Setting.IsCollectAllShaders)
			{
				AssetBundleCollectorSettingData.ModifyShader(isCollectAllShader, AssetBundleCollectorSettingData.Setting.ShadersBundleName);
			}
			if (isCollectAllShader)
			{
				string shadersBundleName = EditorGUILayout.TextField("AssetBundle名称", AssetBundleCollectorSettingData.Setting.ShadersBundleName);
				if (shadersBundleName != AssetBundleCollectorSettingData.Setting.ShadersBundleName)
				{
					AssetBundleCollectorSettingData.ModifyShader(isCollectAllShader, shadersBundleName);
				}
			}

			// 列表显示
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"[ Collector ]");
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Collectors.Count; i++)
			{
				var collector = AssetBundleCollectorSettingData.Setting.Collectors[i];
				string directory = collector.CollectDirectory;
				string packRuleClassName = collector.PackRuleClassName;
				string filterRuleClassName = collector.FilterRuleClassName;
				bool dontWriteAssetPath = collector.DontWriteAssetPath;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(directory);

					// IPackRule
					{
						int index = PackRuleClassNameToIndex(packRuleClassName);
						int newIndex = EditorGUILayout.Popup(index, _packRuleClassArray, GUILayout.MaxWidth(150));
						if (newIndex != index)
						{
							packRuleClassName = IndexToPackRuleClassName(newIndex);
							AssetBundleCollectorSettingData.ModifyCollector(directory, packRuleClassName, filterRuleClassName, dontWriteAssetPath);
						}
					}

					// IFilterRule
					{
						int index = FilterRuleClassNameToIndex(filterRuleClassName);
						int newIndex = EditorGUILayout.Popup(index, _filterRuleClassArray, GUILayout.MaxWidth(150));
						if (newIndex != index)
						{
							filterRuleClassName = IndexToFilterRuleClassName(newIndex);
							AssetBundleCollectorSettingData.ModifyCollector(directory, packRuleClassName, filterRuleClassName, dontWriteAssetPath);
						}
					}

					// DontWriteAssetPath
					bool newToggleValue = EditorGUILayout.Toggle("DontWriteAssetPath", dontWriteAssetPath, GUILayout.MaxWidth(180));
					if (newToggleValue != dontWriteAssetPath)
					{
						AssetBundleCollectorSettingData.ModifyCollector(directory, packRuleClassName, filterRuleClassName, newToggleValue);
					}

					if (GUILayout.Button("-", GUILayout.MaxWidth(40)))
					{
						AssetBundleCollectorSettingData.RemoveCollector(directory);
						break;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();

			// 添加按钮
			if (GUILayout.Button("+"))
			{
				string resultPath = EditorTools.OpenFolderPanel("Select Folder", _lastOpenFolderPath);
				if (resultPath != null)
				{
					_lastOpenFolderPath = EditorTools.AbsolutePathToAssetPath(resultPath);
					string defaultPackRuleClassName = nameof(PackExplicit);
					string defaultFilterRuleClassName = nameof(CollectAll);
					bool defaultDontWriteAssetPathValue = false;
					AssetBundleCollectorSettingData.AddCollector(_lastOpenFolderPath, defaultPackRuleClassName, defaultFilterRuleClassName, defaultDontWriteAssetPathValue);
				}
			}

			// 导入配置按钮
			if (GUILayout.Button("Import Config"))
			{
				string resultPath = EditorTools.OpenFilePath("Select Folder", _lastOpenFolderPath);
				if (resultPath != null)
				{
					CollectorConfigImporter.ImportXmlConfig(resultPath);
				}
			}
		}
		private void OnDrawDLC()
		{
			// 列表显示
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"[ DLC ]");
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.DLCFiles.Count; i++)
			{
				string filePath = AssetBundleCollectorSettingData.Setting.DLCFiles[i];
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(filePath);
					if (GUILayout.Button("-", GUILayout.MaxWidth(40)))
					{
						AssetBundleCollectorSettingData.RemoveDLC(filePath);
						break;
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			// 添加按钮
			if (GUILayout.Button("+"))
			{
				string resultPath = EditorTools.OpenFilePath("Select File", "Assets/");
				if (resultPath != null)
				{
					string filePath = EditorTools.AbsolutePathToAssetPath(resultPath);
					AssetBundleCollectorSettingData.AddDLC(filePath);
				}
			}
		}
	}
}