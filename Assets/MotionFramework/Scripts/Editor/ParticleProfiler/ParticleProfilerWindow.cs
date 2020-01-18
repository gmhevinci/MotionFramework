//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MotionFramework.Editor
{
	public class ParticleProfilerWindow : EditorWindow
	{
		static ParticleProfilerWindow _thisInstance;

		[MenuItem("MotionTools/Particle Profiler", false, 201)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(ParticleProfilerWindow), false, "特效分析器", true) as ParticleProfilerWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}

			_thisInstance.Show();
		}

		/// <summary>
		/// 特效预制体
		/// </summary>
		private UnityEngine.Object _effectPrefab = null;

		/// <summary>
		/// 预览所使用的场景
		/// </summary>
		private UnityEditor.SceneAsset _profilerScene = null;

		/// <summary>
		/// 粒子测试类
		/// </summary>
		private ParticleProfiler _profiler = new ParticleProfiler();

		// GUI相关
		private bool _isPause = false;
		private double _lastTime = 0;
		private Vector2 _scrollPos1 = Vector2.zero;
		private Vector2 _scrollPos2 = Vector2.zero;
		private Vector2 _scrollPos3 = Vector2.zero;
		private bool _isShowCurves = true;
		private bool _isShowTextures = false;
		private bool _isShowMeshs = false;
		private bool _isShowTips = false;
		private Texture2D _texTips = null;

		private const string PROFILER_SCENE_KEY = "ParticleProfilerWindow_ScenePath";


		private void Awake()
		{
			_lastTime = EditorApplication.timeSinceStartup;

			// 加载提示图片
			string folderPath = EditorTools.FindFolder(Application.dataPath, "ParticleProfiler");
			if (string.IsNullOrEmpty(folderPath) == false)
			{
				string temp = EditorTools.AbsolutePathToAssetPath(folderPath);
				_texTips = AssetDatabase.LoadAssetAtPath<Texture2D>($"{temp}/GUI/tips.png");
				if (_texTips == null)
					Debug.LogWarning("Not found ParticleProfilerWindows tips texture.");
			}
			else
			{
				Debug.LogWarning("Not found ParticleProfiler folder.");
			}

			// 加载测试场景
			string path = EditorPrefs.GetString(PROFILER_SCENE_KEY, string.Empty);
			if (string.IsNullOrEmpty(path) == false)
				_profilerScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
		}
		private void OnGUI()
		{
			EditorGUILayout.Space();

			// 测试场景
			SceneAsset scene = (SceneAsset)EditorGUILayout.ObjectField($"测试场景", _profilerScene, typeof(SceneAsset), false);
			if (_profilerScene != scene)
			{
				_profilerScene = scene;
				string path = AssetDatabase.GetAssetPath(scene);
				EditorPrefs.SetString(PROFILER_SCENE_KEY, path);
			}

			// 测试特效
			_effectPrefab = EditorGUILayout.ObjectField($"请选择特效", _effectPrefab, typeof(UnityEngine.Object), false);

			// 测试按钮
			if (GUILayout.Button("测试"))
			{
				if (CheckProfilerCondition() == false)
					return;

				// 焦点锁定游戏窗口
				var gameViewType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
				EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
				gameView.Focus();

				// 开始分析
				_isPause = false;
				_profiler.Analyze(_effectPrefab);
				Debug.Log($"开始测试特效：{_effectPrefab.name}");
			}

			// 暂停按钮
			if (_isPause)
			{
				if (GUILayout.Button("点击按钮恢复"))
					_isPause = false;
			}
			else
			{
				if (GUILayout.Button("点击按钮暂停"))
					_isPause = true;
			}

			// 粒子基本信息
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"材质数量：{_profiler.MaterialCount}");
			EditorGUILayout.LabelField($"纹理数量：{_profiler.TextureCount}");
			EditorGUILayout.LabelField($"纹理内存：{EditorUtility.FormatBytes(_profiler.TextureMemory)}");
			EditorGUILayout.LabelField($"粒子系统组件：{_profiler.ParticleSystemComponentCount} 个");

			// 粒子动态信息
			EditorGUILayout.Space();
			EditorGUILayout.LabelField($"DrawCall：{_profiler.DrawCallCurrentNum}  最大：{_profiler.DrawCallMaxNum}");
			EditorGUILayout.LabelField($"粒子数量：{_profiler.ParticleCurrentCount}  最大：{_profiler.ParticleMaxCount}");
			EditorGUILayout.LabelField($"三角面数：{_profiler.TriangleCurrentCount}  最大：{_profiler.TriangleMaxCount}");

			// 错误信息
			if (_profiler.Errors.Count > 0)
			{
				EditorGUILayout.Space();
				EditorGUILayout.HelpBox($"请修正以下错误提示", MessageType.Error, true);
				EditorGUI.indentLevel = 1;
				foreach (var error in _profiler.Errors)
				{
					GUIStyle style = new GUIStyle();
					style.normal.textColor = new Color(0.8f, 0, 0);
					EditorGUILayout.LabelField(error, style);
				}
				EditorGUI.indentLevel = 0;
			}

			// 曲线图
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowCurves = EditorGUILayout.Foldout(_isShowCurves, "时间曲线");
				if (_isShowCurves)
				{
					float curveHeight = 80;
					EditorGUI.indentLevel = 1;
					EditorGUILayout.LabelField($"采样时长 {_profiler.CurveSampleTime} 秒");
					EditorGUILayout.CurveField("DrawCall", _profiler.DrawCallCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("粒子数量", _profiler.ParticleCountCurve, GUILayout.Height(curveHeight));
					EditorGUILayout.CurveField("三角面数", _profiler.TriangleCountCurve, GUILayout.Height(curveHeight));
					EditorGUI.indentLevel = 0;
				}
			}

			// 纹理列表
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowTextures = EditorGUILayout.Foldout(_isShowTextures, "纹理列表");
				if (_isShowTextures)
				{
					EditorGUI.indentLevel = 1;
					_scrollPos1 = EditorGUILayout.BeginScrollView(_scrollPos1);
					{
						List<Texture> textures = _profiler.AllTextures;
						foreach (var tex in textures)
						{
							EditorGUILayout.LabelField($"{tex.name}  尺寸:{tex.height }*{tex.width}  格式:{ParticleProfiler.GetTextureFormatString(tex)}");
							EditorGUILayout.ObjectField("", tex, typeof(Texture), false, GUILayout.Width(80));
						}
					}
					EditorGUILayout.EndScrollView();
					EditorGUI.indentLevel = 0;
				}
			}

			// 网格列表
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowMeshs = EditorGUILayout.Foldout(_isShowMeshs, "网格列表");
				if (_isShowMeshs)
				{
					EditorGUI.indentLevel = 1;
					_scrollPos2 = EditorGUILayout.BeginScrollView(_scrollPos2);
					{
						List<Mesh> meshs = _profiler.AllMeshs;
						foreach (var mesh in meshs)
						{
							EditorGUILayout.ObjectField($"三角面数 : {mesh.triangles.Length / 3}", mesh, typeof(MeshFilter), false, GUILayout.Width(300));
						}
					}
					EditorGUILayout.EndScrollView();
					EditorGUI.indentLevel = 0;
				}
			}

			// 过程化检测结果
			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				_isShowTips = EditorGUILayout.Foldout(_isShowTips, "过程化检测结果");
				if (_isShowTips)
				{
					EditorGUI.indentLevel = 1;
					_scrollPos3 = EditorGUILayout.BeginScrollView(_scrollPos3);
					{
						GUILayout.Button(_texTips); //绘制提示图片
						EditorGUILayout.HelpBox($"以下粒子系统组件不支持过程化模式！具体原因查看气泡提示", MessageType.Warning, true);
#if UNITY_2018_4_OR_NEWER
						List<ParticleSystem> particleList = _profiler.AllParticles;
						foreach (var ps in particleList)
						{
							if (ps.proceduralSimulationSupported == false)
								EditorGUILayout.ObjectField($"{ps.gameObject.name}", ps.gameObject, typeof(GameObject), false, GUILayout.Width(300));
						}
#else
					EditorGUILayout.LabelField("当前版本不支持过程化检测，请升级至2018.4版本或最新版本");
#endif
					}
					EditorGUILayout.EndScrollView();
					EditorGUI.indentLevel = 0;
				}
			}
		}
		private void OnDestroy()
		{
			_profiler.DestroyPrefab();
		}
		private void Update()
		{
			float deltaTime = (float)(EditorApplication.timeSinceStartup - _lastTime);
			_lastTime = EditorApplication.timeSinceStartup;

			// 更新测试特效
			if (_isPause == false)
				_profiler.Update(deltaTime);

			// 刷新窗口界面
			this.Repaint();
		}

		/// <summary>
		/// 检测特效测试条件
		/// </summary>
		/// <returns>如果通过返回TRUE</returns>
		private bool CheckProfilerCondition()
		{
			if (_effectPrefab == null)
			{
				Debug.LogWarning("请指定一个要测试的特效预制体！");
				return false;
			}

			if (_profilerScene == null)
			{
				Debug.LogWarning("建议设置一个专门的特效场景测试！");
				return true;
			}

			UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			if (activeScene.name == _profilerScene.name)
				return true;

			// 注意：如果当前场景未保存
			if (activeScene.isDirty)
			{
				EditorUtility.DisplayDialog("警告", $"请先保存当前场景：{activeScene.name}", "确定");
				return false;
			}

			// 打开特效测试场景
			string path = AssetDatabase.GetAssetPath(_profilerScene);
			Debug.Log($"打开特效测试场景：{path}");
			EditorSceneManager.OpenScene(path);
			return true;
		}
	}
}