//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class AssetImporterWindow : EditorWindow
	{
		static AssetImporterWindow _thisInstance;

		[MenuItem("MotionTools/Asset Importer", false, 102)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(AssetImporterWindow), false, "资源导入工具", true) as AssetImporterWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}

			_thisInstance.Show();
		}

		/// <summary>
		/// 上次打开的文件夹路径
		/// </summary>
		private string _lastOpenFolderPath = "Assets/";

		/// <summary>
		/// 资源处理器类列表
		/// </summary>
		private string[] _processorClassArray = null;

		// 初始化相关
		private bool _isInit = false;
		private void Init()
		{
			// 字典KEY转换为数组
			List<string> keyList = new List<string>();
			foreach (var pair in ImportSettingData.CacheTypes)
			{
				keyList.Add(pair.Key);
			}
			_processorClassArray = keyList.ToArray();
		}
		private int NameToIndex(string name)
		{
			for (int i = 0; i < _processorClassArray.Length; i++)
			{
				if (_processorClassArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToName(int index)
		{
			for (int i = 0; i < _processorClassArray.Length; i++)
			{
				if (i == index)
					return _processorClassArray[i];
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

			// 列表显示
			EditorGUILayout.Space();
			for (int i = 0; i < ImportSettingData.Setting.Elements.Count; i++)
			{
				string folderPath = ImportSettingData.Setting.Elements[i].FolderPath;
				string processorName = ImportSettingData.Setting.Elements[i].ProcessorName;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(folderPath);

					int index = NameToIndex(processorName);
					int newIndex = EditorGUILayout.Popup(index, _processorClassArray, GUILayout.MaxWidth(150));
					if (newIndex != index)
					{
						string processClassName = IndexToName(newIndex);
						ImportSettingData.ModifyElement(folderPath, processClassName);
					}

					if (GUILayout.Button("-", GUILayout.MaxWidth(40)))
					{
						ImportSettingData.RemoveElement(folderPath);
						break;
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			// 添加按钮
			if (GUILayout.Button("+"))
			{
				string resultPath = EditorTools.OpenFolderPanel("+", _lastOpenFolderPath);
				if (resultPath != null)
				{
					_lastOpenFolderPath = EditorTools.AbsolutePathToAssetPath(resultPath);
					ImportSettingData.AddElement(_lastOpenFolderPath);
				}
			}
		}
	}
}