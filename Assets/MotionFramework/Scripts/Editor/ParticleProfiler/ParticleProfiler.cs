//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class ParticleProfiler
	{
		private MethodInfo _calculateEffectUIDataMethod = null;

		// 克隆的预制体
		private GameObject _cloneObject = null;

		private readonly List<ParticleSystem> _allParticles = new List<ParticleSystem>(100);
		private readonly List<Material> _allMaterials = new List<Material>(100);
		private readonly List<Texture> _allTextures = new List<Texture>(100);
		private readonly List<Mesh> _allMeshs = new List<Mesh>(100);
		private readonly List<string> _errors = new List<string>(100);

		private double _beginTime = 0;
		private float _repeterTimer = 0;

		// 材质信息
		public int MaterialCount { private set; get; }

		// 纹理信息
		public long TextureMemory { private set; get; }
		public int TextureCount { private set; get; }

		// DrawCall信息
		public int DrawCallCurrentNum { private set; get; }
		public int DrawCallMaxNum { private set; get; }

		// 粒子信息
		public int ParticleCurrentCount { private set; get; }
		public int ParticleMaxCount { private set; get; }

		// 三角面信息
		public int TriangleCurrentCount { private set; get; }
		public int TriangleMaxCount { private set; get; }

		// 填充率信息
		public int OverdrawTotalPixel { private set; get; }
		public int OverdrawPerPixel { private set; get; }

		// 曲线图相关
		public readonly AnimationCurve DrawCallCurve = new AnimationCurve();
		public readonly AnimationCurve ParticleCountCurve = new AnimationCurve();
		public readonly AnimationCurve TriangleCountCurve = new AnimationCurve();

		/// <summary>
		/// 曲线采样总时长（秒）
		/// </summary>
		public float CurveSampleTime { private set; get; } = 1f;

		/// <summary>
		/// 粒子系统组件总数
		/// </summary>
		public int ParticleSystemComponentCount
		{
			get
			{
				if (_allParticles == null)
					return 0;
				else
					return _allParticles.Count;
			}
		}

		// 获取集合数据相关
		public List<ParticleSystem> AllParticles
		{
			get { return _allParticles; }
		}
		public List<Texture> AllTextures
		{
			get { return _allTextures; }
		}
		public List<Material> AllMaterials
		{
			get { return _allMaterials; }
		}
		public List<Mesh> AllMeshs
		{
			get { return _allMeshs; }
		}
		public List<string> Errors
		{
			get { return _errors; }
		}


		public ParticleProfiler()
		{
			_calculateEffectUIDataMethod = typeof(ParticleSystem).GetMethod("CalculateEffectUIData", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		/// <summary>
		/// 开始分析特效
		/// </summary>
		/// <param name="prefab">特效预制体</param>
		public void Analyze(UnityEngine.Object prefab)
		{
			if (prefab == null)
				return;

			// 销毁旧的克隆体
			DestroyPrefab();

			// 重置数据
			Reset();

			// 创建新实例对象
			_cloneObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

			// 分析显示相关数据
			ParseRendererInfo(_cloneObject);

			_beginTime = EditorApplication.timeSinceStartup;
		}

		/// <summary>
		/// 重置所有数据
		/// </summary>
		public void Reset()
		{
			_allParticles.Clear();
			_allMaterials.Clear();
			_allTextures.Clear();
			_allMeshs.Clear();
			_errors.Clear();

			_beginTime = 0;
			_repeterTimer = 0;

			MaterialCount = 0;
			TextureMemory = 0;
			TextureCount = 0;
			DrawCallCurrentNum = 0;
			DrawCallMaxNum = 0;
			ParticleCurrentCount = 0;
			ParticleMaxCount = 0;
			TriangleCurrentCount = 0;
			TriangleMaxCount = 0;
			OverdrawTotalPixel = 0;
			OverdrawPerPixel = 0;

			for (int i = DrawCallCurve.length - 1; i >= 0; i--)
			{
				DrawCallCurve.RemoveKey(i);
			}
			for (int i = ParticleCountCurve.length - 1; i >= 0; i--)
			{
				ParticleCountCurve.RemoveKey(i);
			}
			for (int i = TriangleCountCurve.length - 1; i >= 0; i--)
			{
				TriangleCountCurve.RemoveKey(i);
			}
		}

		/// <summary>
		/// 销毁克隆的预制体
		/// </summary>
		public void DestroyPrefab()
		{
			if (_cloneObject != null)
			{
				GameObject.DestroyImmediate(_cloneObject);
				_cloneObject = null;
			}
		}

		/// <summary>
		/// 更新分析器
		/// </summary>
		public void Update(float deltaTime)
		{
			if (_cloneObject == null)
				return;

			UpdateSimulate(deltaTime);
			UpdateRuntimeStats();
			UpdateRuntimeParticleCount();
			UpdateAllAnimationCurve(deltaTime);
		}


		/// <summary>
		/// 分析显示相关数据
		/// </summary>
		private void ParseRendererInfo(GameObject go)
		{
			// 获取所有的粒子系统组件
			_cloneObject.GetComponentsInChildren<ParticleSystem>(true, _allParticles);

			// 获取所有唯一的材质球
			_allMaterials.Clear();
			Renderer[] rendererList = go.GetComponentsInChildren<Renderer>(true);
			foreach (var rd in rendererList)
			{
				if (rd.enabled == false)
				{
					ParticleSystem ps = rd.gameObject.GetComponent<ParticleSystem>();
					if (ps.emission.enabled)
						_errors.Add($"{rd.gameObject.name} 粒子系统组件：Renderer未启用，请关闭Emission发射器！");
				}

				foreach (var mat in rd.sharedMaterials)
				{
					if (mat == null)
					{
						if (rd.enabled)
							_errors.Add($"{rd.gameObject.name} 粒子系统组件：Renderer已启用，但是缺少材质球！");
						continue;
					}

					if (_allMaterials.Contains(mat) == false)
						_allMaterials.Add(mat);
				}
			}

			// 获取所有唯一的纹理
			_allTextures.Clear();
			foreach (var mat in _allMaterials)
			{
				int count = ShaderUtil.GetPropertyCount(mat.shader);
				for (int i = 0; i < count; i++)
				{
					ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.GetPropertyType(mat.shader, i);
					if (propertyType == ShaderUtil.ShaderPropertyType.TexEnv)
					{
						string propertyName = ShaderUtil.GetPropertyName(mat.shader, i);
						Texture tex = mat.GetTexture(propertyName);
						if (tex != null)
						{
							if (_allTextures.Contains(tex) == false)
								_allTextures.Add(tex);
						}
					}
				}
			}
			foreach (var ps in _allParticles)
			{
				ParticleSystem.TextureSheetAnimationModule tm = ps.textureSheetAnimation;
				if (tm.mode == ParticleSystemAnimationMode.Sprites)
				{
					for (int i = 0; i < tm.spriteCount; i++)
					{
						Sprite sprite = tm.GetSprite(i);
						if (sprite != null && sprite.texture != null)
						{
							if (_allTextures.Contains(sprite.texture) == false)
								_allTextures.Add(sprite.texture);
						}
					}
				}
			}

			// 获取所有唯一的网格
			_allMeshs.Clear();
			MeshFilter[] list1 = go.GetComponentsInChildren<MeshFilter>();
			foreach (var meshFilter in list1)
			{
				if (meshFilter.sharedMesh != null)
				{
					if (_allMeshs.Contains(meshFilter.sharedMesh) == false)
						_allMeshs.Add(meshFilter.sharedMesh);
				}
			}
			SkinnedMeshRenderer[] list2 = go.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (var skinMesh in list2)
			{
				if (skinMesh.sharedMesh != null)
				{
					if (_allMeshs.Contains(skinMesh.sharedMesh) == false)
						_allMeshs.Add(skinMesh.sharedMesh);
				}
			}
			foreach (var ps in _allParticles)
			{
				var psr = ps.GetComponent<ParticleSystemRenderer>();
				if (psr != null && psr.renderMode == ParticleSystemRenderMode.Mesh)
				{
					if (psr.mesh != null)
					{
						if (_allMeshs.Contains(psr.mesh) == false)
							_allMeshs.Add(psr.mesh);
					}
				}
			}

			// 计算材质数量
			MaterialCount = _allMaterials.Count;

			// 计算纹理数量和所需内存大小	
			TextureCount = _allTextures.Count;
			TextureMemory = 0;
			foreach (var tex in _allTextures)
			{
				TextureMemory += GetStorageMemorySize(tex);
			}

			// 计算特效生命周期
			CurveSampleTime = 1f;
			foreach (var ps in _allParticles)
			{
				float playingTime = ps.main.duration;
				float delayTime = GetMaxTime(ps.main.startDelay);
				float lifeTime = GetMaxTime(ps.main.startLifetime);
				if ((delayTime + lifeTime) > playingTime)
					playingTime = delayTime + lifeTime;
				if (playingTime > CurveSampleTime)
					CurveSampleTime = playingTime;
			}
		}
		private float GetMaxTime(ParticleSystem.MinMaxCurve curveInfo)
		{
			if (curveInfo.mode == ParticleSystemCurveMode.Constant)
				return curveInfo.constant;
			else if (curveInfo.mode == ParticleSystemCurveMode.TwoConstants)
				return curveInfo.constantMax;
			else if (curveInfo.mode == ParticleSystemCurveMode.Curve)
				return GetAnimationCurveMaxValue(curveInfo.curve) * curveInfo.curveMultiplier;
			else if (curveInfo.mode == ParticleSystemCurveMode.TwoCurves)
				return GetAnimationCurveMaxValue(curveInfo.curveMin, curveInfo.curveMax) * curveInfo.curveMultiplier;
			else
				throw new System.NotImplementedException($"{curveInfo.mode}");
		}
		private float GetAnimationCurveMaxValue(AnimationCurve curve)
		{
			float maxValue = float.MinValue;
			foreach (var key in curve.keys)
			{
				if (key.value > maxValue)
					maxValue = key.value;
			}
			return maxValue;
		}
		private float GetAnimationCurveMaxValue(AnimationCurve minCurve, AnimationCurve maxCurve)
		{
			float value1 = GetAnimationCurveMaxValue(minCurve);
			float value2 = GetAnimationCurveMaxValue(maxCurve);
			if (value1 > value2)
				return value1;
			else
				return value2;
		}

		/// <summary>
		/// 模拟粒子效果
		/// </summary>
		private void UpdateSimulate(float deltaTime)
		{
			foreach (var ps in _allParticles)
			{
				ps.Simulate(deltaTime, false, false);
			}
		}

		/// <summary>
		/// 更新运行时的Stats信息
		/// </summary>
		private void UpdateRuntimeStats()
		{
			// 注意：如果开启填充率测试，这里要除以2
			DrawCallCurrentNum = UnityEditor.UnityStats.batches;
			if (DrawCallCurrentNum > DrawCallMaxNum)
				DrawCallMaxNum = DrawCallCurrentNum;

			TriangleCurrentCount = UnityEditor.UnityStats.triangles;
			if (TriangleCurrentCount > TriangleMaxCount)
				TriangleMaxCount = TriangleCurrentCount;
		}

		/// <summary>
		/// 更新运行时的粒子数量信息
		/// </summary>
		private void UpdateRuntimeParticleCount()
		{
			ParticleCurrentCount = 0;
			foreach (var ps in _allParticles)
			{
				int count = GetRuntimeParticleCount(ps);
				ParticleCurrentCount += count;
			}
			if (ParticleCurrentCount > ParticleMaxCount)
				ParticleMaxCount = ParticleCurrentCount;
		}
		private int GetRuntimeParticleCount(ParticleSystem ps)
		{
			// 获取粒子系统运行时的数量
			int count = 0;
			object[] parameters = new object[] { count, 0.0f, Mathf.Infinity };
			_calculateEffectUIDataMethod.Invoke(ps, parameters);
			count = (int)parameters[0];
			return count;
		}

		/// <summary>
		/// 更新所有曲线图
		/// </summary>
		private void UpdateAllAnimationCurve(float deltaTime)
		{
			float time = (float)(EditorApplication.timeSinceStartup - _beginTime);
			if (time > CurveSampleTime)
				return;

			_repeterTimer += deltaTime;
			if (_repeterTimer > 0.03333f)
			{
				_repeterTimer = 0;
				ParticleCountCurve.AddKey(time, ParticleCurrentCount);
				DrawCallCurve.AddKey(time, DrawCallCurrentNum);
				TriangleCountCurve.AddKey(time, TriangleCurrentCount);
			}
		}


		#region 静态方法
		/// <summary>
		/// 获取纹理运行时内存大小
		/// </summary>
		public static int GetStorageMemorySize(Texture tex)
		{
			int size = (int)InvokeStaticMethod("UnityEditor.TextureUtil", "GetStorageMemorySize", tex);
			return size;
		}

		/// <summary>
		/// 获取当前平台纹理的格式
		/// </summary>
		public static string GetTextureFormatString(Texture tex)
		{
			TextureFormat format = (TextureFormat)InvokeStaticMethod("UnityEditor.TextureUtil", "GetTextureFormat", tex);
			return format.ToString();
		}

		private static object InvokeStaticMethod(string type, string method, params object[] parameters)
		{
			var assembly = typeof(AssetDatabase).Assembly;
			var temp = assembly.GetType(type);
			var methodInfo = temp.GetMethod(method, BindingFlags.Public | BindingFlags.Static);
			return methodInfo.Invoke(null, parameters);
		}
		#endregion
	}
}