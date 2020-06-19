//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class UISettingData
	{
		public static UISetting Setting;

		static UISettingData()
		{
			// 加载配置文件
			Setting = AssetDatabase.LoadAssetAtPath<UISetting>(EditorDefine.UISettingFilePath);
			if (Setting == null)
			{
				Debug.LogWarning($"Create new ImportSetting.asset : {EditorDefine.UISettingFilePath}");
				Setting = ScriptableObject.CreateInstance<UISetting>();
				EditorTools.CreateFileDirectory(EditorDefine.UISettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.UISettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log("Load ImportSetting.asset ok");
			}
		}
	}
}