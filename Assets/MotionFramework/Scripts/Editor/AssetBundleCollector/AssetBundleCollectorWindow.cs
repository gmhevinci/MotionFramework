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

		private void OnGUI()
		{
			// 列表显示
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"Collection List");
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Elements.Count; i++)
			{
				string folderPath = AssetBundleCollectorSettingData.Setting.Elements[i].FolderPath;
				AssetBundleCollectorSetting.EFolderPackRule packRule = AssetBundleCollectorSettingData.Setting.Elements[i].PackRule;
				AssetBundleCollectorSetting.EBundleLabelRule labelRule = AssetBundleCollectorSettingData.Setting.Elements[i].LabelRule;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(folderPath);

					AssetBundleCollectorSetting.EFolderPackRule newPackRule = (AssetBundleCollectorSetting.EFolderPackRule)EditorGUILayout.EnumPopup(packRule, GUILayout.MaxWidth(150));
					if (newPackRule != packRule)
					{
						packRule = newPackRule;
						AssetBundleCollectorSettingData.ModifyElement(folderPath, packRule, labelRule);
					}

					AssetBundleCollectorSetting.EBundleLabelRule newLabelRule = (AssetBundleCollectorSetting.EBundleLabelRule)EditorGUILayout.EnumPopup(labelRule, GUILayout.MaxWidth(150));
					if (newLabelRule != labelRule)
					{
						labelRule = newLabelRule;
						AssetBundleCollectorSettingData.ModifyElement(folderPath, packRule, labelRule);
					}

					if (GUILayout.Button("-", GUILayout.MaxWidth(40)))
					{
						AssetBundleCollectorSettingData.RemoveElement(folderPath);
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