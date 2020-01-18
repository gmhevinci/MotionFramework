//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class AssetSearchWindow : EditorWindow
	{
		static AssetSearchWindow _thisInstance;

		[MenuItem("MotionTools/Asset Search", false, 103)]
		static void Init()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(AssetSearchWindow), false, "资源引用搜索工具", true) as AssetSearchWindow;
				_thisInstance.minSize = new Vector2(400, 600);
			}

			_thisInstance.Show();
		}


		/// <summary>
		/// 默认的搜索路径
		/// </summary>
		private const string DEFAULT_SEARCH_PATH = "Assets/Works/Resources";

		/// <summary>
		/// 显示集合
		/// </summary>
		private Dictionary<EAssetFileExtension, List<string>> _collection = null;

		/// <summary>
		/// 搜索目录
		/// </summary>
		private string _searchFolderPath = DEFAULT_SEARCH_PATH;

		/// <summary>
		/// 搜索类型
		/// </summary>
		private EAssetSearchType _serachType = EAssetSearchType.Prefab;

		/// <summary>
		/// 搜索对象
		/// </summary>
		private UnityEngine.Object _searchObject = null;

		/// <summary>
		/// GUI滑动条位置
		/// </summary>
		private Vector2 _scrollPos = Vector2.zero;


		private void OnGUI()
		{
			// 搜索路径
			EditorGUILayout.Space();
			if (GUILayout.Button("设置搜索目录", GUILayout.MaxWidth(80)))
			{
				string resultPath = EditorTools.OpenFolderPanel("搜索目录", _searchFolderPath);
				if (resultPath != null)
					_searchFolderPath = EditorTools.AbsolutePathToAssetPath(resultPath);
			}
			EditorGUILayout.LabelField($"搜索目录：{_searchFolderPath}");

			// 搜索类型
			_serachType = (EAssetSearchType)EditorGUILayout.EnumPopup("搜索类型", _serachType);

			// 搜索目标
			_searchObject = EditorGUILayout.ObjectField($"搜索目标", _searchObject, typeof(UnityEngine.Object), false);
			if (_searchObject != null && _searchObject.GetType() == typeof(Texture2D))
			{
				string assetPath = AssetDatabase.GetAssetPath(_searchObject.GetInstanceID());
				var spriteObject = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
				if (spriteObject != null)
					_searchObject = spriteObject;
			}

			// 执行搜索
			EditorGUILayout.Space();
			if (GUILayout.Button("搜索"))
			{
				if (_searchObject == null)
				{
					EditorUtility.DisplayDialog("错误", "请先设置搜索目标", "确定");
					return;
				}
				string assetPath = AssetDatabase.GetAssetPath(_searchObject.GetInstanceID());
				FindReferenceInProject(assetPath, _searchFolderPath, _serachType);
			}
			EditorGUILayout.Space();

			// 如果字典类为空
			if (_collection == null)
				return;

			// 如果收集列表为空
			if (CheckCollectIsEmpty())
			{
				EditorGUILayout.LabelField("提示：没有找到任何被依赖资源。");
				return;
			}

			_scrollPos = GUILayout.BeginScrollView(_scrollPos);

			// 显示依赖列表
			foreach (EAssetFileExtension value in Enum.GetValues(typeof(EAssetFileExtension)))
			{
				List<string> collect = _collection[value];
				if (collect.Count == 0)
					continue;

				string HeaderName = value.ToString();
				if (value == EAssetFileExtension.prefab)
					HeaderName = "预制体";
				else if (value == EAssetFileExtension.fbx)
					HeaderName = "模型";
				else if (value == EAssetFileExtension.cs)
					HeaderName = "脚本";
				else if (value == EAssetFileExtension.png || value == EAssetFileExtension.jpg)
					HeaderName = "图片";
				else if (value == EAssetFileExtension.mat)
					HeaderName = "材质球";
				else if (value == EAssetFileExtension.shader)
					HeaderName = "着色器";
				else if (value == EAssetFileExtension.ttf)
					HeaderName = "字体";
				else if (value == EAssetFileExtension.anim)
					HeaderName = "动画";
				else if (value == EAssetFileExtension.unity)
					HeaderName = "场景";
				else
					throw new NotImplementedException(value.ToString());

				// 绘制头部
				if (EditorTools.DrawHeader(HeaderName))
				{
					// 绘制预制体专属按钮
					if (value == EAssetFileExtension.prefab)
					{
						if (GUILayout.Button("过滤", GUILayout.MaxWidth(80)))
							FilterReference();
						if (GUILayout.Button("FindAll", GUILayout.MaxWidth(80)))
							FindAll(collect);
					}

					// 绘制依赖列表
					foreach (string item in collect)
					{
						EditorGUILayout.BeginHorizontal();
						UnityEngine.Object go = AssetDatabase.LoadAssetAtPath(item, typeof(UnityEngine.Object));
						if (value == EAssetFileExtension.unity)
							EditorGUILayout.ObjectField(go, typeof(SceneView), true);
						else
							EditorGUILayout.ObjectField(go, typeof(UnityEngine.Object), false);

						// 绘制预制体专属按钮
						if (value == EAssetFileExtension.prefab)
						{
							if (GUILayout.Button("Find", GUILayout.MaxWidth(80)))
								FindOne(go as GameObject);
						}
						EditorGUILayout.EndHorizontal();
					}
				}
			}

			GUILayout.EndScrollView();
		}

		/// <summary>
		/// 获取资源被依赖的集合
		/// </summary>
		/// <param name="searchAssetPath">资源路径</param>
		/// <param name="searchFolder">搜索的文件夹</param>
		private void FindReferenceInProject(string searchAssetPath, string searchFolder, EAssetSearchType serachType)
		{
			ShowProgress(0, 0, 0);

			// 创建集合
			if (_collection == null)
			{
				_collection = new Dictionary<EAssetFileExtension, List<string>>();
				foreach (EAssetFileExtension value in Enum.GetValues(typeof(EAssetFileExtension)))
				{
					_collection.Add(value, new List<string>());
				}
			}

			// 清空集合
			foreach (EAssetFileExtension value in Enum.GetValues(typeof(EAssetFileExtension)))
			{
				_collection[value].Clear();
			}

			// 搜索相关资源的GUID
			string[] allGuids = null;
			if (serachType == EAssetSearchType.All)
				allGuids = AssetDatabase.FindAssets(string.Empty, new string[] { $"{searchFolder}" });
			else
				allGuids = AssetDatabase.FindAssets($"t:{serachType}", new string[] { $"{searchFolder}" });

			// 查找引用
			int curCount = 0;
			foreach (string guid in allGuids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				string[] dpends = AssetDatabase.GetDependencies(path, false);
				foreach (string name in dpends)
				{
					if (name.Equals(searchAssetPath))
					{
						foreach (EAssetFileExtension value in Enum.GetValues(typeof(EAssetFileExtension)))
						{
							if (path.EndsWith($".{value}"))
							{
								if (_collection[value].Contains(path) == false)
								{
									_collection[value].Add(path);
									break;
								}
							}
						}
					}
				}
				curCount++;
				ShowProgress((float)curCount / (float)allGuids.Length, allGuids.Length, curCount);
			}

			EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// 检测收集列表是否为空
		/// </summary>
		private bool CheckCollectIsEmpty()
		{
			bool isEmpty = true;
			foreach (EAssetFileExtension value in Enum.GetValues(typeof(EAssetFileExtension)))
			{
				List<string> list = _collection[value];
				if (list.Count > 0)
				{
					isEmpty = false;
					break;
				}
			}
			return isEmpty;
		}

		/// <summary>
		/// 过滤Hierarchy窗口
		/// </summary>
		private void FilterReference()
		{
			Selection.activeObject = _searchObject;
			EditorApplication.ExecuteMenuItem("Assets/Find References In Scene");
		}

		private void FindAll(List<string> collect)
		{
			List<GameObject> prefabList = new List<GameObject>();
			foreach (string path in collect)
			{
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
				prefabList.Add(prefab as GameObject);
			}
			FindInternal(prefabList.ToArray());
		}
		private void FindOne(GameObject prefab)
		{
			List<GameObject> prefabList = new List<GameObject>();
			prefabList.Add(prefab);
			FindInternal(prefabList.ToArray());
		}
		private void FindInternal(GameObject[] prefabs)
		{
			GameObject canvasRoot = FindCanvasRootInScene();
			if (canvasRoot == null)
				Debug.LogWarning("Not found canvas root in scene.");

			// 获取场景里的预制体引用，如果不存在就克隆一个预制体
			for (int i = 0; i < prefabs.Length; i++)
			{
				GameObject prefab = prefabs[i];
				GameObject cloneObject = EditorTools.GetClonePrefabInScene(prefab);
				if (cloneObject == null)
				{
					cloneObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
					if (cloneObject == null)
						cloneObject = GameObject.Instantiate<GameObject>(prefab);

					// 如果是UI面板就放到Canvas底下
					var bhvUI = cloneObject.GetComponent<Canvas>();
					if (bhvUI != null)
					{
						if (canvasRoot != null)
							cloneObject.transform.SetParent(canvasRoot.transform, false);
					}
				}
			}

			EditorTools.FindReferencesInPrefabs(_searchObject, prefabs);
		}
		private GameObject FindCanvasRootInScene()
		{
			GameObject[] findObjects = GameObject.FindObjectsOfType<GameObject>();
			if (findObjects.Length == 0)
				return null;

			for (int i = 0; i < findObjects.Length; i++)
			{
				GameObject findObject = findObjects[i];
				Transform rootTemp = findObject.transform.root;
				Canvas canvas = rootTemp.GetComponent<Canvas>();
				if (canvas != null)
					return rootTemp.gameObject;
			}

			return null;
		}

		private void ShowProgress(float progress, int total, int cur)
		{
			EditorUtility.DisplayProgressBar("Searching", $"Finding ({cur}/{total}), please waiting...", progress);
		}
	}
}