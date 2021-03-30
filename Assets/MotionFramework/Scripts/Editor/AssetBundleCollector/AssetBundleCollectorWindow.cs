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

		private string[] _labelClassArray = null;
		private string[] _filterClassArray = null;

		// 初始化相关
		private bool _isInit = false;
		private void Init()
		{
			List<string> labelClassNames = AssetBundleCollectorSettingData.GetLabelClassNames();
			_labelClassArray = labelClassNames.ToArray();

			List<string> filterClassNames = AssetBundleCollectorSettingData.GetFilterClassNames();
			_filterClassArray = filterClassNames.ToArray();
		}
		private int LabelClassNameToIndex(string name)
		{
			for (int i = 0; i < _labelClassArray.Length; i++)
			{
				if (_labelClassArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToLabelClassName(int index)
		{
			for (int i = 0; i < _labelClassArray.Length; i++)
			{
				if (i == index)
					return _labelClassArray[i];
			}
			return string.Empty;
		}		
		private int FilterClassNameToIndex(string name)
		{
			for (int i = 0; i < _filterClassArray.Length; i++)
			{
				if (_filterClassArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToFilterClassName(int index)
		{
			for (int i = 0; i < _filterClassArray.Length; i++)
			{
				if (i == index)
					return _filterClassArray[i];
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
			if(isCollectAllShader != AssetBundleCollectorSettingData.Setting.IsCollectAllShaders)
			{
				AssetBundleCollectorSettingData.ModifyShader(isCollectAllShader, AssetBundleCollectorSettingData.Setting.ShadersBundleName);
			}
			if(isCollectAllShader)
			{
				string shadersBundleName = EditorGUILayout.TextField("AssetBundle名称", AssetBundleCollectorSettingData.Setting.ShadersBundleName);
				if(shadersBundleName != AssetBundleCollectorSettingData.Setting.ShadersBundleName)
				{
					AssetBundleCollectorSettingData.ModifyShader(isCollectAllShader, shadersBundleName);
				}
			}

			// 列表显示
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"[ Collector ]");
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Collectors.Count; i++)
			{
				var collector = AssetBundleCollectorSettingData.Setting.Collectors[i];
				string directory = collector.CollectDirectory;
				string labelClassName = collector.LabelClassName;
				string filterClassName = collector.FilterClassName;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(directory);

					// 标签类
					{
						int index = LabelClassNameToIndex(labelClassName);
						int newIndex = EditorGUILayout.Popup(index, _labelClassArray, GUILayout.MaxWidth(150));
						if (newIndex != index)
						{
							labelClassName = IndexToLabelClassName(newIndex);
							AssetBundleCollectorSettingData.ModifyCollector(directory, labelClassName, filterClassName);
						}
					}

					// 过滤类
					{
						int index = FilterClassNameToIndex(filterClassName);
						int newIndex = EditorGUILayout.Popup(index, _filterClassArray, GUILayout.MaxWidth(150));
						if (newIndex != index)
						{
							filterClassName = IndexToFilterClassName(newIndex);
							AssetBundleCollectorSettingData.ModifyCollector(directory, labelClassName, filterClassName);
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