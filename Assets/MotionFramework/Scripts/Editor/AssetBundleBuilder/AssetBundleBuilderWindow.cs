//--------------------------------------------------
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

		// 构建参数
		public int BuildVersion;
		public BuildTarget BuildTarget;

		// 构建选项
		public ECompressOption CompressOption = ECompressOption.Uncompressed;
		public bool IsForceRebuild = false;

		// GUI相关
		private bool _isInit = false;
		private GUIStyle _centerStyle;
		private GUIStyle _leftStyle;
		private bool _showToolsFoldout = true;

		// 构建器
		private readonly AssetBundleBuilder _assetBuilder = new AssetBundleBuilder();


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
			BuildVersion = appVersion.Revision;
			BuildTarget = EditorUserBuildSettings.activeBuildTarget;

			// 读取配置
			LoadSettingsFromPlayerPrefs();
		}
		private void OnGUI()
		{
			// 初始化
			InitInternal();

			// 标题
			EditorGUILayout.LabelField("Build setup", _centerStyle);
			EditorGUILayout.Space();

			// 输出路径
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRootPath();
			string outputDirectory = AssetBundleBuilderHelper.MakeOutputDirectory(defaultOutputRoot, BuildTarget);
			EditorGUILayout.LabelField("Build Output", outputDirectory);

			BuildVersion = EditorGUILayout.IntField("Build Version", BuildVersion, GUILayout.MaxWidth(250));
			CompressOption = (ECompressOption)EditorGUILayout.EnumPopup("Compression", CompressOption, GUILayout.MaxWidth(250));
			IsForceRebuild = GUILayout.Toggle(IsForceRebuild, "Froce Rebuild", GUILayout.MaxWidth(120));

			// 构建按钮
			EditorGUILayout.Space();
			if (GUILayout.Button("Build", GUILayout.MaxHeight(40)))
			{
				string title;
				string content;
				if (IsForceRebuild)
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
					// 清空控制台
					EditorTools.ClearUnityConsole();

					// 存储配置
					SaveSettingsToPlayerPrefs();

					EditorApplication.delayCall += ExecuteBuild;
				}
				else
				{
					Debug.LogWarning("[Build] 打包已经取消");
				}
			}

			// 绘制工具栏部分
			OnDrawGUITools();
		}
		private void OnDrawGUITools()
		{
			GUILayout.Space(50);
			using (new EditorGUI.DisabledScope(false))
			{
				_showToolsFoldout = EditorGUILayout.Foldout(_showToolsFoldout, "Tools");
				if (_showToolsFoldout)
				{
					EditorGUILayout.Space();

					// 检测所有损坏的预制体文件
					if (GUILayout.Button("Check Invalid Prefabs", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += AssetBundleBuilderTools.CheckCorruptionPrefab;
					}

					// 清理无用的材质球属性
					if (GUILayout.Button("Clear Material Unused Property", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += AssetBundleBuilderTools.ClearMaterialUnusedProperty;
					}

					// 拷贝补丁文件到流目录
					if (GUILayout.Button("Copy Patch To StreamingAssets", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += () => { AssetBundleBuilderTools.CopyPatchFilesToStreamming(true, BuildTarget); };
					}
				}
			}
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		private void ExecuteBuild()
		{
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRootPath();
			AssetBundleBuilder.BuildParameters buildParameters = new AssetBundleBuilder.BuildParameters();
			buildParameters.OutputRoot = defaultOutputRoot;
			buildParameters.BuildTarget = BuildTarget;
			buildParameters.BuildVersion = BuildVersion;
			buildParameters.CompressOption = CompressOption;
			buildParameters.IsForceRebuild = IsForceRebuild;
			_assetBuilder.Run(buildParameters);
		}

		#region 设置相关
		private const string StrEditorCompressOption = "StrEditorCompressOption";
		private const string StrEditorIsForceRebuild = "StrEditorIsForceRebuild";

		/// <summary>
		/// 存储配置
		/// </summary>
		private void SaveSettingsToPlayerPrefs()
		{
			EditorTools.PlayerSetEnum<ECompressOption>(StrEditorCompressOption, CompressOption);
			EditorTools.PlayerSetBool(StrEditorIsForceRebuild, IsForceRebuild);
		}

		/// <summary>
		/// 读取配置
		/// </summary>
		private void LoadSettingsFromPlayerPrefs()
		{
			CompressOption = EditorTools.PlayerGetEnum<ECompressOption>(StrEditorCompressOption, ECompressOption.Uncompressed);
			IsForceRebuild = EditorTools.PlayerGetBool(StrEditorIsForceRebuild, false);
		}
		#endregion
	}
}