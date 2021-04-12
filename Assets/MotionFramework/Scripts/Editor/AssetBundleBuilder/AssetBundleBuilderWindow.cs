//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
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
		public bool IsAppendHash = false;
		public bool IsDisableWriteTypeTree = false;
		public bool IsIgnoreTypeTreeChanges = false;

		// GUI相关
		private bool _isInit = false;
		private GUIStyle _centerStyle;
		private GUIStyle _leftStyle;
		private bool _showSettingFoldout = true;
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

			// 构建版本
			BuildVersion = EditorGUILayout.IntField("Build Version", BuildVersion, GUILayout.MaxWidth(250));

			// 输出路径
			string defaultOutputRoot = AssetBundleBuilderHelper.GetDefaultOutputRootPath();
			string outputDirectory = AssetBundleBuilder.MakeOutputDirectory(defaultOutputRoot, BuildTarget);
			EditorGUILayout.LabelField("Build Output", outputDirectory);

			// 构建选项
			EditorGUILayout.Space();
			IsForceRebuild = GUILayout.Toggle(IsForceRebuild, "Froce Rebuild", GUILayout.MaxWidth(120));

			// 高级选项
			using (new EditorGUI.DisabledScope(false))
			{
				EditorGUILayout.Space();
				_showSettingFoldout = EditorGUILayout.Foldout(_showSettingFoldout, "Advanced Settings");
				if (_showSettingFoldout)
				{
					int indent = EditorGUI.indentLevel;
					EditorGUI.indentLevel = 1;
					CompressOption = (ECompressOption)EditorGUILayout.EnumPopup("Compression", CompressOption);
					IsAppendHash = EditorGUILayout.ToggleLeft("Append Hash", IsAppendHash, GUILayout.MaxWidth(120));
					IsDisableWriteTypeTree = EditorGUILayout.ToggleLeft("Disable Write Type Tree", IsDisableWriteTypeTree, GUILayout.MaxWidth(200));
					IsIgnoreTypeTreeChanges = EditorGUILayout.ToggleLeft("Ignore Type Tree Changes", IsIgnoreTypeTreeChanges, GUILayout.MaxWidth(200));
					EditorGUI.indentLevel = indent;
				}
			}

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

					// 检测所有损坏的无效的预制体
					if (GUILayout.Button("Check Invalid Prefabs", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += CheckAllPrefabValid;
					}

					// 清理无用的材质球属性
					if (GUILayout.Button("Clear Material Unused Property", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += ClearMaterialUnusedProperty;
					}

					// 清空并拷贝所有补丁包到StreamingAssets目录
					if (GUILayout.Button("Copy Patch To StreamingAssets", GUILayout.MaxWidth(250), GUILayout.MaxHeight(40)))
					{
						EditorApplication.delayCall += RefreshStreammingFolder;
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
			_assetBuilder.SetBuildParameters(defaultOutputRoot, BuildTarget, BuildVersion);
			_assetBuilder.SetBuildOptions(CompressOption, IsForceRebuild, IsAppendHash, IsDisableWriteTypeTree, IsIgnoreTypeTreeChanges);
			_assetBuilder.Run();
		}

		/// <summary>
		/// 检测预制件是否损坏
		/// </summary>
		private void CheckAllPrefabValid()
		{
			// 获取所有的打包路径
			List<string> collectDirectorys = AssetBundleCollectorSettingData.GetAllCollectDirectory();
			if (collectDirectorys.Count == 0)
				throw new Exception("[BuildPackage] 打包路径列表不能为空");

			// 获取所有资源列表
			int checkCount = 0;
			int invalidCount = 0;
			string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Prefab}", collectDirectorys.ToArray());
			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
				if (prefab == null)
				{
					invalidCount++;
					Debug.LogError($"[Build] 发现损坏预制件：{assetPath}");
				}

				// 进度条相关
				checkCount++;
				EditorUtility.DisplayProgressBar("进度", $"检测预制件文件是否损坏：{checkCount}/{guids.Length}", (float)checkCount / guids.Length);
			}

			EditorUtility.ClearProgressBar();
			if (invalidCount == 0)
				Debug.Log($"没有发现损坏预制件");
		}

		/// <summary>
		/// 清理无用的材质球属性
		/// </summary>
		private void ClearMaterialUnusedProperty()
		{
			// 获取所有的打包路径
			List<string> collectDirectorys = AssetBundleCollectorSettingData.GetAllCollectDirectory();
			if (collectDirectorys.Count == 0)
				throw new Exception("[BuildPackage] 打包路径列表不能为空");

			// 获取所有资源列表
			int checkCount = 0;
			int removedCount = 0;
			string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Material}", collectDirectorys.ToArray());
			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
				bool removed = EditorTools.ClearMaterialUnusedProperty(mat);
				if (removed)
				{
					removedCount++;
					Debug.LogWarning($"[Build] 材质球已被处理：{assetPath}");
				}

				// 进度条相关
				checkCount++;
				EditorUtility.DisplayProgressBar("进度", $"清理无用的材质球属性：{checkCount}/{guids.Length}", (float)checkCount / guids.Length);
			}

			EditorUtility.ClearProgressBar();
			if (removedCount == 0)
				Debug.Log($"没有发现冗余的材质球属性");
			else
				AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// 刷新流目录
		/// </summary>
		private void RefreshStreammingFolder()
		{
			string streamingDirectory = Application.dataPath + "/StreamingAssets";
			EditorTools.ClearFolder(streamingDirectory);

			string outputRoot = AssetBundleBuilderHelper.GetDefaultOutputRootPath();
			AssetBundleBuilderHelper.CopyPackageToStreamingFolder(BuildTarget, outputRoot);
		}

		#region 设置相关
		private const string StrEditorCompressOption = "StrEditorCompressOption";
		private const string StrEditorIsForceRebuild = "StrEditorIsForceRebuild";
		private const string StrEditorIsAppendHash = "StrEditorIsAppendHash";
		private const string StrEditorIsDisableWriteTypeTree = "StrEditorIsDisableWriteTypeTree";
		private const string StrEditorIsIgnoreTypeTreeChanges = "StrEditorIsIgnoreTypeTreeChanges";
		private const string StrEditorIsUsePlayerSettingVersion = "StrEditorIsUsePlayerSettingVersion";

		/// <summary>
		/// 存储配置
		/// </summary>
		private void SaveSettingsToPlayerPrefs()
		{
			EditorTools.PlayerSetEnum<ECompressOption>(StrEditorCompressOption, CompressOption);
			EditorTools.PlayerSetBool(StrEditorIsForceRebuild, IsForceRebuild);
			EditorTools.PlayerSetBool(StrEditorIsAppendHash, IsAppendHash);
			EditorTools.PlayerSetBool(StrEditorIsDisableWriteTypeTree, IsDisableWriteTypeTree);
			EditorTools.PlayerSetBool(StrEditorIsIgnoreTypeTreeChanges, IsIgnoreTypeTreeChanges);
		}

		/// <summary>
		/// 读取配置
		/// </summary>
		private void LoadSettingsFromPlayerPrefs()
		{
			CompressOption = EditorTools.PlayerGetEnum<ECompressOption>(StrEditorCompressOption, ECompressOption.Uncompressed);
			IsForceRebuild = EditorTools.PlayerGetBool(StrEditorIsForceRebuild, false);
			IsAppendHash = EditorTools.PlayerGetBool(StrEditorIsAppendHash, false);
			IsDisableWriteTypeTree = EditorTools.PlayerGetBool(StrEditorIsDisableWriteTypeTree, false);
			IsIgnoreTypeTreeChanges = EditorTools.PlayerGetBool(StrEditorIsIgnoreTypeTreeChanges, false);
		}
		#endregion
	}
}