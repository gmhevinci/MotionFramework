//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
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

		/// <summary>
		/// 资源处理器类列表
		/// </summary>
		private string[] _collectorClassArray = null;

		// 初始化相关
		private bool _isInit = false;
		private void Init()
		{
			List<string> names = AssetBundleCollectorSettingData.GetCollectorNames();
			_collectorClassArray = names.ToArray();
		}
		private int NameToIndex(string name)
		{
			for (int i = 0; i < _collectorClassArray.Length; i++)
			{
				if (_collectorClassArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToName(int index)
		{
			for (int i = 0; i < _collectorClassArray.Length; i++)
			{
				if (i == index)
					return _collectorClassArray[i];
			}
			return string.Empty;
		}

		private void OnGUI()
		{
			// 初始化
			if (_isInit == false)
			{
				_isInit = true;
				Init();
			}

			// 列表显示
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"Collection List");
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Elements.Count; i++)
			{
				string directory = AssetBundleCollectorSettingData.Setting.Elements[i].CollectDirectory;
				AssetBundleCollectorSetting.ECollectRule packRule = AssetBundleCollectorSettingData.Setting.Elements[i].CollectRule;
				string collectorName = AssetBundleCollectorSettingData.Setting.Elements[i].CollectorName;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(directory);

					AssetBundleCollectorSetting.ECollectRule newPackRule = (AssetBundleCollectorSetting.ECollectRule)EditorGUILayout.EnumPopup(packRule, GUILayout.MaxWidth(150));
					if (newPackRule != packRule)
					{
						packRule = newPackRule;
						AssetBundleCollectorSettingData.ModifyElement(directory, packRule, collectorName);
					}

					int index = NameToIndex(collectorName);
					int newIndex = EditorGUILayout.Popup(index, _collectorClassArray, GUILayout.MaxWidth(150));
					if (newIndex != index)
					{
						string newCollectorName = IndexToName(newIndex);
						AssetBundleCollectorSettingData.ModifyElement(directory, packRule, newCollectorName);
					}

					if (GUILayout.Button("-", GUILayout.MaxWidth(40)))
					{
						AssetBundleCollectorSettingData.RemoveElement(directory);
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
					AssetBundleCollectorSettingData.AddElement(_lastOpenFolderPath);
				}
			}
		}
	}
}