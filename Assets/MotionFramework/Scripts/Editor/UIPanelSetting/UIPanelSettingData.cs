//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class UIPanelSettingData
	{
		public static UIPanelSetting Setting;

		static UIPanelSettingData()
		{
			// 加载配置文件
			Setting = AssetDatabase.LoadAssetAtPath<UIPanelSetting>(EditorDefine.UIPanelSettingFilePath);
			if (Setting == null)
			{
				Debug.LogWarning($"Create new ImportSetting.asset : {EditorDefine.UIPanelSettingFilePath}");
				Setting = ScriptableObject.CreateInstance<UIPanelSetting>();
				EditorTools.CreateFileDirectory(EditorDefine.UIPanelSettingFilePath);
				AssetDatabase.CreateAsset(Setting, EditorDefine.UIPanelSettingFilePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log("Load ImportSetting.asset ok");
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

		/// <summary>
		/// 设置精灵文件夹路径
		/// </summary>
		public static void SetUISpriteDirectory(string directory)
		{
			Setting.UISpriteDirectory = directory;
			SaveFile();
		}

		/// <summary>
		/// 设置图集文件夹路径
		/// </summary>
		public static void SetUIAtlasDirectory(string directory)
		{
			Setting.UIAtlasDirectory = directory;
			SaveFile();
		}

		/// <summary>
		/// 检测配置文件有效性
		/// </summary>
		public static bool CheckValid()
		{
			if (Setting == null)
			{
				Debug.LogError($"{nameof(UIPanelSetting)} is not load.");
				return false;
			}

			if (string.IsNullOrEmpty(Setting.UISpriteDirectory))
			{
				Debug.LogError($"{nameof(Setting.UISpriteDirectory)} is emptry. Open MotionTools -> {nameof(UIPanelSettingWindow)}");
				return false;
			}
			if (string.IsNullOrEmpty(Setting.UIAtlasDirectory))
			{
				Debug.LogError($"{nameof(Setting.UIAtlasDirectory)} is emptry. Open MotionTools -> {nameof(UIPanelSettingWindow)}");
				return false;
			}

			if (Directory.Exists(Setting.UISpriteDirectory) == false)
			{
				Debug.LogError($"The directory is not found : {Setting.UISpriteDirectory} Open MotionTools -> {nameof(UIPanelSettingWindow)}");
				return false;
			}
			if (Directory.Exists(Setting.UIAtlasDirectory) == false)
			{
				Debug.LogError($"The directory is not found : {Setting.UIAtlasDirectory} Open MotionTools -> {nameof(UIPanelSettingWindow)}");
				return false;
			}

			return true;
		}
	}
}