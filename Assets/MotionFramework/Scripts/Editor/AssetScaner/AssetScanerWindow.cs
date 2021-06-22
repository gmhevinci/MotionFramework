//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MotionFramework.Editor
{
	public class AssetScanerWindow : EditorWindow
	{
		static AssetScanerWindow _thisInstance;

		[MenuItem("MotionTools/Asset Scaner", false, 105)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(AssetScanerWindow), false, "资源扫描工具", true) as AssetScanerWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		/// <summary>
		/// 上次打开的文件夹路径
		/// </summary>
		private string _lastOpenFolderPath = "Assets/";

		/// <summary>
		/// 类列表
		/// </summary>
		private string[] _classArray = null;

		// GUI相关
		private Vector2 _scrollPos = new Vector2();
		private Dictionary<string, List<ScanReport>> _results;

		// 初始化相关
		private bool _isInit = false;
		private void Init()
		{
			List<string> names = AssetScanerSettingData.GetScanerNames();
			_classArray = names.ToArray();
		}
		private int NameToIndex(string name)
		{
			for (int i = 0; i < _classArray.Length; i++)
			{
				if (_classArray[i] == name)
					return i;
			}
			return 0;
		}
		private string IndexToName(int index)
		{
			for (int i = 0; i < _classArray.Length; i++)
			{
				if (i == index)
					return _classArray[i];
			}
			return string.Empty;
		}

		void OnGUI()
		{
			// 初始化
			if (_isInit == false)
			{
				_isInit = true;
				Init();
			}

			// 列表显示
			EditorGUILayout.Space();
			for (int i = 0; i < AssetScanerSettingData.Setting.Elements.Count; i++)
			{
				string directory = AssetScanerSettingData.Setting.Elements[i].ScanerDirectory;
				string scanerName = AssetScanerSettingData.Setting.Elements[i].ScanerName;

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField(directory);

					int index = NameToIndex(scanerName);
					int newIndex = EditorGUILayout.Popup(index, _classArray, GUILayout.MaxWidth(150));
					if (newIndex != index)
					{
						string newScanerName = IndexToName(newIndex);
						AssetScanerSettingData.ModifyElement(directory, newScanerName);
					}

					if (GUILayout.Button("-", GUILayout.MaxWidth(40)))
					{
						AssetScanerSettingData.RemoveElement(directory);
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
					AssetScanerSettingData.AddElement(_lastOpenFolderPath);
				}
			}

			// 扫描按钮
			if (GUILayout.Button("Scan"))
			{
				_results = AssetScanerSettingData.ScanAllAssets();
			}

			// 绘制扫描结果
			DrawResults();
		}
		private void DrawResults()
		{
			if (_results == null || _results.Count == 0)
				return;

			_scrollPos = GUILayout.BeginScrollView(_scrollPos);

			// 显示依赖列表
			foreach (var valuePair in _results)
			{
				// 绘制头部
				string HeaderName = valuePair.Key;
				if (EditorTools.DrawHeader(HeaderName))
				{
					// 绘制报告列表
					foreach (var report in valuePair.Value)
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField($"名称: {report.AssetObject.name} 报告: {report.ReportInfo}");
						if(report.AssetObject.GetType() == typeof(UnityEngine.Texture) || report.AssetObject.GetType() == typeof(UnityEngine.Texture2D))
							EditorGUILayout.ObjectField("", report.AssetObject, typeof(UnityEngine.Texture), false, GUILayout.Width(80));
						else
							EditorGUILayout.ObjectField("", report.AssetObject, typeof(UnityEngine.Object), false, GUILayout.Width(80));
						EditorGUILayout.EndHorizontal();
					}
				}
			}

			GUILayout.EndScrollView();
		}
	}
}