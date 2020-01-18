//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework
{
	public static class MotionEngine
	{
		private class ModuleWrapper
		{
			public int Priority { private set; get; }
			public IModule Module { private set; get; }

			public ModuleWrapper(IModule module, int priority)
			{
				Module = module;
				Priority = priority;
			}
		}

		private static readonly List<ModuleWrapper> _coms = new List<ModuleWrapper>(100);
		private static MonoBehaviour _behaviour;
		private static bool _isDirty = false;


		/// <summary>
		/// 初始化框架
		/// </summary>
		public static void Initialize(MonoBehaviour behaviour)
		{
			if (_behaviour != null)
				throw new Exception($"{nameof(MotionEngine)} is already initialized.");

			_behaviour = behaviour;
		}

		/// <summary>
		/// 更新框架
		/// </summary>
		public static void Update()
		{
			// 如果有新模块需要重新排序
			if (_isDirty)
			{
				_isDirty = false;
				_coms.Sort((left, right) =>
				{
					if (left.Priority > right.Priority)
						return -1;
					else if (left.Priority == right.Priority)
						return 0;
					else
						return 1;
				});
			}

			// 轮询所有模块
			for (int i = 0; i < _coms.Count; i++)
			{
				_coms[i].Module.OnUpdate();
			}
		}

		/// <summary>
		/// GUI绘制
		/// </summary>
		public static void DrawGUI()
		{
			for (int i = 0; i < _coms.Count; i++)
			{
				_coms[i].Module.OnGUI();
			}
		}
		
		/// <summary>
		/// 查询游戏模块是否存在
		/// </summary>
		public static bool Contains(System.Type moduleType)
		{
			for (int i = 0; i < _coms.Count; i++)
			{
				if (_coms[i].Module.GetType() == moduleType)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 创建游戏模块
		/// </summary>
		/// <typeparam name="T">模块类</typeparam>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public static T CreateModule<T>(int priority = 0) where T : class, IModule
		{
			return CreateModule<T>(null, priority);
		}

		/// <summary>
		/// 创建游戏模块
		/// </summary>
		/// <typeparam name="T">模块类</typeparam>
		/// <param name="createParam">创建参数</param>
		/// <param name="priority">运行时的优先级，优先级越大越早执行。如果没有设置优先级，那么会按照添加顺序执行</param>
		public static T CreateModule<T>(System.Object createParam, int priority = 0) where T : class, IModule
		{
			if (priority < 0)
				throw new Exception("The priority can not be negative");

			if (Contains(typeof(T)))
				throw new Exception($"Game module {typeof(T)} is already existed");

			// 如果没有设置优先级
			if (priority == 0)
			{
				int minPriority = GetMinPriority();
				priority = --minPriority;
			}

			MotionLog.Log(ELogLevel.Log, $"Create game module : {typeof(T)}");
			T module = Activator.CreateInstance<T>();
			ModuleWrapper wrapper = new ModuleWrapper(module, priority);
			wrapper.Module.OnCreate(createParam);
			_coms.Add(wrapper);
			_isDirty = true;
			return module;
		}

		/// <summary>
		/// 获取游戏模块
		/// </summary>
		/// <typeparam name="T">模块类</typeparam>
		public static T GetModule<T>() where T : class, IModule
		{
			System.Type type = typeof(T);
			for (int i = 0; i < _coms.Count; i++)
			{
				if (_coms[i].Module.GetType() == type)
					return _coms[i].Module as T;
			}

			MotionLog.Log(ELogLevel.Warning, $"Not found game module {type}");
			return null;
		}

		/// <summary>
		/// 获取当前模块里最小的优先级
		/// </summary>
		private static int GetMinPriority()
		{
			int minPriority = 0;
			for (int i = 0; i < _coms.Count; i++)
			{
				if (_coms[i].Priority < minPriority)
					minPriority = _coms[i].Priority;
			}
			return minPriority; //小于等于零
		}

		#region 协程相关
		/// <summary>
		/// 开启一个协程
		/// </summary>
		public static Coroutine StartCoroutine(IEnumerator coroutine)
		{
			if (_behaviour == null)
				throw new Exception($"{nameof(MotionEngine)} is not initialize. Use MotionEngine.Initialize");
			return _behaviour.StartCoroutine(coroutine);
		}

		/// <summary>
		/// 停止一个协程
		/// </summary>
		/// <param name="coroutine"></param>
		public static void StopCoroutine(Coroutine coroutine)
		{
			if (_behaviour == null)
				throw new Exception($"{nameof(MotionEngine)} is not initialize. Use MotionEngine.Initialize");
			_behaviour.StopCoroutine(coroutine);
		}

		/// <summary>
		/// 停止所有协程
		/// </summary>
		public static void StopAllCoroutines()
		{
			if (_behaviour == null)
				throw new Exception($"{nameof(MotionEngine)} is not initialize. Use MotionEngine.Initialize");
			_behaviour.StopAllCoroutines();
		}
		#endregion
	}
}