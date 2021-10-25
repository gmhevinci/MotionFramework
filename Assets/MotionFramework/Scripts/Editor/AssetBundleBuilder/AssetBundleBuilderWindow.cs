﻿//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class AssetBundleBuilderWindow : EditorWindow
	{
		static AssetBundleBuilderWindow _thisInstance;

		[MenuItem("MotionTools/AssetBundle Builder", false, 102)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(AssetBundleBuilderWindow), false, "资源包构建工具", true) as AssetBundleBuilderWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		// 构建器
		private readonly AssetBundleBuilder _assetBuilder = new AssetBundleBuilder();

		// 构建参数
		private int _buildVersion;
		private BuildTarget _buildTarget;
		private ECompressOption _compressOption = ECompressOption.Uncompressed;
		private bool _isForceRebuild = false;
		private string _buildinTags = string.Empty;

		// GUI相关
		private bool _isInit = false;
		private GUIStyle _centerStyle;
		private GUIStyle _leftStyle;
		private bool _showToolsFoldout = true;


		private void OnGUI()
		{
			InitInternal();
			OnDrawBuildContent();
			OnDrawGUITools();
		}
		private void InitInternal()
		{
			if (_isInit)
				return;
			_isInit = true;

			// GUI相关
			_centerStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
			_centerStyle.alignment = TextAnchor.UpperCenter;
			_leftStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
			_leftStyle.alignment = TextAnchor.MiddleLeft;

			// 构建参数
			var appVersion = new Version(Application.version);
			_buildVersion = appVersion.Revision;
			_buildTarget = EditorUserBuildSettings.activeBuildTarget;

			// 读取配置
			LoadSettingsFromPlayerPrefs();
		}
		private void OnDrawBuildContent()
		{
			// 标题
			EditorGUILayout.LabelField("Build setup", _centerStyle);
			EditorGUILayout.Space();

			// 输出路径
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			string pipelineOutputDirectory = AssetBundleBuilderHelper.MakePipelineOutputDirectory(defaultOutputRoot, _buildTarget);
			EditorGUILayout.LabelField("Build Output", pipelineOutputDirectory);

			// 构建参数
			_buildVersion = EditorGUILayout.IntField("Build Version", _buildVersion, GUILayout.MaxWidth(250));
			_compressOption = (ECompressOption)EditorGUILayout.EnumPopup("Compression", _compressOption, GUILayout.MaxWidth(250));
			_isForceRebuild = GUILayout.Toggle(_isForceRebuild, "Force Rebuild", GUILayout.MaxWidth(120));
			if(_isForceRebuild)
				_buildinTags = EditorGUILayout.TextField("Buildin Tags", _buildinTags);

			// 构建按钮
			EditorGUILayout.Space();
			if (GUILayout.Button("Build", GUILayout.MaxHeight(40)))
			{
				string title;
				string content;
				if (_isForceRebuild)
				{
					title = "警告";
					content = "确定开始强制构建吗，这样会删除所有已有构建的文件";
				}
				else
				{
					title = "提示";
					content = "确定开始增量构建吗";
				}
				if (EditorUtility.DisplayDialog(title, content, "Yes", "No"))
				{
					SaveSettingsToPlayerPrefs();
					EditorTools.ClearUnityConsole();			
					EditorApplication.delayCall += ExecuteBuild;
				}
				else
				{
					Debug.LogWarning("[Build] 打包已经取消");
				}
			}
		}
		private void OnDrawGUITools()
		{
			GUILayout.Space(50);
			using (new EditorGUI.DisabledScope(false))
			{
				_showToolsFoldout = EditorGUILayout.Foldout(_showToolsFoldout, "工具");
				if (_showToolsFoldout)
				{
					EditorGUILayout.Space();

					if (GUILayout.Button("检测损坏的预制体", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += AssetBundleBuilderTools.CheckCorruptionPrefab;
					}

					if (GUILayout.Button("检测动画控制器的冗余状态", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += AssetBundleBuilderTools.FindRedundantAnimationState;
					}

					if (GUILayout.Button("清理材质球的冗余属性", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += AssetBundleBuilderTools.ClearMaterialUnusedProperty;
					}
				}
			}
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		private void ExecuteBuild()
		{
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRoot();
			AssetBundleBuilder.BuildParameters buildParameters = new AssetBundleBuilder.BuildParameters();
			buildParameters.IsVerifyBuildingResult = true;
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = _buildTarget;
			buildParameters.BuildVersion = _buildVersion;
			buildParameters.CompressOption = _compressOption;
			buildParameters.IsForceRebuild = _isForceRebuild;
			buildParameters.BuildinTags = _buildinTags;
			_assetBuilder.Run(buildParameters);
		}

		#region 配置相关
		private const string StrEditorCompressOption = "StrEditorCompressOption";
		private const string StrEditorIsForceRebuild = "StrEditorIsForceRebuild";
		private const string StrEditorBuildinTags = "StrEditorBuildinTags";

		/// <summary>
		/// 存储配置
		/// </summary>
		private void SaveSettingsToPlayerPrefs()
		{
			EditorTools.PlayerSetEnum<ECompressOption>(StrEditorCompressOption, _compressOption);
			EditorPrefs.SetBool(StrEditorIsForceRebuild, _isForceRebuild);
			EditorPrefs.SetString(StrEditorBuildinTags, _buildinTags);
		}

		/// <summary>
		/// 读取配置
		/// </summary>
		private void LoadSettingsFromPlayerPrefs()
		{
			_compressOption = EditorTools.PlayerGetEnum<ECompressOption>(StrEditorCompressOption, ECompressOption.Uncompressed);
			_isForceRebuild = EditorPrefs.GetBool(StrEditorIsForceRebuild, false);
			_buildinTags = EditorPrefs.GetString(StrEditorBuildinTags, string.Empty);
		}
		#endregion
	}
}