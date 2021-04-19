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

		private string[] _bundleLabelClassArray = null;
		private string[] _searchFilterClassArray = null;
		private Vector2 _scrollPos = Vector2.zero;

		// 初始化相关
		private bool _isInit = false;
		private void Init()
		{
			List<string> bundleLablelClassNames = AssetBundleCollectorSettingData.GetBundleLabelClassNames();
			_bundleLabelClassArray = bundleLablelClassNames.ToArray();

			List<string> searchFilterClassNames = AssetBundleCollectorSettingData.GetSearchFilterClassNames();
			_searchFilterClassArray = searchFilterClassNames.ToArray();
		}
		private int BundleLabelClassNameToIndex(string name)
		{
			for (int i = 0; i < _bundleLabelClassArray.Length; i++)
			{
				if (_bundleLabelClassArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToBundleLabelClassName(int index)
		{
			for (int i = 0; i < _bundleLabelClassArray.Length; i++)
			{
				if (i == index)
					return _bundleLabelClassArray[i];
			}
			return string.Empty;
		}
		private int SearchFilterClassNameToIndex(string name)
		{
			for (int i = 0; i < _searchFilterClassArray.Length; i++)
			{
				if (_searchFilterClassArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToSearchFilterClassName(int index)
		{
			for (int i = 0; i < _searchFilterClassArray.Length; i++)
			{
				if (i == index)
					return _searchFilterClassArray[i];
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
				string bundleLabelClassName = collector.BundleLabelClassName;
				string searchFilterClassName = collector.SearchFilterClassName;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(directory);

					// IBundleLabel
					{
						int index = BundleLabelClassNameToIndex(bundleLabelClassName);
						int newIndex = EditorGUILayout.Popup(index, _bundleLabelClassArray, GUILayout.MaxWidth(200));
						if (newIndex != index)
						{
							bundleLabelClassName = IndexToBundleLabelClassName(newIndex);
							AssetBundleCollectorSettingData.ModifyCollector(directory, bundleLabelClassName, searchFilterClassName);
						}
					}

					// ISearchFilter
					{
						int index = SearchFilterClassNameToIndex(searchFilterClassName);
						int newIndex = EditorGUILayout.Popup(index, _searchFilterClassArray, GUILayout.MaxWidth(150));
						if (newIndex != index)
						{
							searchFilterClassName = IndexToSearchFilterClassName(newIndex);
							AssetBundleCollectorSettingData.ModifyCollector(directory, bundleLabelClassName, searchFilterClassName);
						}
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
					AssetBundleCollectorSettingData.AddCollector(_lastOpenFolderPath);
				}
			}

			// 导入配置按钮
			if (GUILayout.Button("Import Config"))
			{
				string resultPath = EditorTools.OpenFilePath("Select Folder", _lastOpenFolderPath);
				if (resultPath != null)
				{
					ConfigFileImporter.ImportXmlConfig(resultPath);
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