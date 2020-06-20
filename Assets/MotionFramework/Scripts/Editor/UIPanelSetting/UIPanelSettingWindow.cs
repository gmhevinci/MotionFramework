//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class UIPanelSettingWindow : EditorWindow
	{
		static UIPanelSettingWindow _thisInstance;

		[MenuItem("MotionTools/UIPanel Setting", false, 202)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(UIPanelSettingWindow), false, "UI面板设置", true) as UIPanelSettingWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();

			// 精灵路径
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("精灵文件夹路径", GUILayout.MaxWidth(150)))
			{
				string resultPath = EditorTools.OpenFolderPanel("Find", UIPanelSettingData.Setting.UISpriteDirectory);
				if (resultPath != null)
				{
					UIPanelSettingData.SetUISpriteDirectory(EditorTools.AbsolutePathToAssetPath(resultPath));
				}
			}
			EditorGUILayout.LabelField($" : {UIPanelSettingData.Setting.UISpriteDirectory}");
			EditorGUILayout.EndHorizontal();

			// 图集路径
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("图集文件夹路径", GUILayout.MaxWidth(150)))
			{
				string resultPath = EditorTools.OpenFolderPanel("Find", UIPanelSettingData.Setting.UIAtlasDirectory);
				if (resultPath != null)
				{
					UIPanelSettingData.SetUIAtlasDirectory(EditorTools.AbsolutePathToAssetPath(resultPath));
				}
			}
			EditorGUILayout.LabelField($" : {UIPanelSettingData.Setting.UIAtlasDirectory}");
			EditorGUILayout.EndHorizontal();
		}
	}
}