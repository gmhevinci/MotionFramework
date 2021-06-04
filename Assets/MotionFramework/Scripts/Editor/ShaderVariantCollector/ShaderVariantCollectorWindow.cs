//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class ShaderVariantCollectionWindow : EditorWindow
	{
		static ShaderVariantCollectionWindow _thisInstance;

		[MenuItem("MotionTools/ShaderVariant Collector", false, 203)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(ShaderVariantCollectionWindow), false, "着色器变种收集工具", true) as ShaderVariantCollectionWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		private string _saveFilePath = "Assets/MyShaderVariants.shadervariants";
		private int _currentShaderCount = 0;
		private int _currentVariantCount = 0;

		private void OnGUI()
		{
			// 文件路径
			EditorGUILayout.Space();
			_saveFilePath = EditorGUILayout.TextField("文件路径", _saveFilePath);

			// 搜集变种
			EditorGUILayout.Space();
			if (GUILayout.Button("搜集变种", GUILayout.MaxWidth(80)))
			{
				EditorApplication.delayCall += CollectShaderVariants;
			}

			EditorGUILayout.LabelField($"CurrentShaderCount : {_currentShaderCount}");
			EditorGUILayout.LabelField($"CurrentVariantCount : {_currentVariantCount}");
		}

		private void CollectShaderVariants()
		{
			ShaderVariantCollector collector = new ShaderVariantCollector();
			collector.Run(_saveFilePath);
			_currentShaderCount = collector.GetCurrentShaderVariantCollectionShaderCount();
			_currentVariantCount = collector.GetCurrentShaderVariantCollectionVariantCount();
		}
	}
}