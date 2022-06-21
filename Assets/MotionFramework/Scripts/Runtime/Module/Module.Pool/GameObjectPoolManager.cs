//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Resource;

namespace MotionFramework.Pool
{
	/// <summary>
	/// 游戏对象池管理器
	/// </summary>
	public sealed class GameObjectPoolManager : ModuleSingleton<GameObjectPoolManager>, IModule
	{
		/// <summary>
		/// 游戏模块创建参数
		/// </summary>
		public class CreateParameters
		{
			/// <summary>
			/// 是否启用惰性对象池
			/// </summary>
			public bool EnableLazyPool = false;

			/// <summary>
			/// 默认的初始容器值
			/// </summary>
			public int DefaultInitCapacity = 0;

			/// <summary>
			/// 默认的最大容器值
			/// </summary>
			public int DefaultMaxCapacity = int.MaxValue;

			/// <summary>
			/// 默认的静默销毁时间
			/// 注意：小于零代表不主动销毁
			/// </summary>
			public float DefaultDestroyTime = -1f;
		}

		private readonly Dictionary<string, GameObjectCollector> _collectors = new Dictionary<string, GameObjectCollector>(100);
		private readonly List<GameObjectCollector> _removeList = new List<GameObjectCollector>(100);
		private GameObject _root;
		private bool _enableLazyPool;
		private int _defaultInitCapacity;
		private int _defaultMaxCapacity;
		private float _defaultDestroyTime;


		void IModule.OnCreate(object createParam)
		{
			// 检测依赖模块
			if (MotionEngine.Contains(typeof(ResourceManager)) == false)
				throw new Exception($"{nameof(GameObjectPoolManager)} depends on {nameof(ResourceManager)}");

			CreateParameters parameters = createParam as CreateParameters;
			if (createParam == null)
				throw new Exception($"{nameof(GameObjectPoolManager)} create param is invalid.");
			if (parameters.DefaultMaxCapacity < parameters.DefaultInitCapacity)
				throw new Exception("The max capacity value must be greater the init capacity value.");

			_enableLazyPool = parameters.EnableLazyPool;
			_defaultInitCapacity = parameters.DefaultInitCapacity;
			_defaultMaxCapacity = parameters.DefaultMaxCapacity;
			_defaultDestroyTime = parameters.DefaultDestroyTime;

			_root = new GameObject("[PoolManager]");
			_root.transform.position = Vector3.zero;
			_root.transform.eulerAngles = Vector3.zero;
			UnityEngine.Object.DontDestroyOnLoad(_root);
		}
		void IModule.OnUpdate()
		{
			_removeList.Clear();
			foreach (var valuePair in _collectors)
			{
				var collector = valuePair.Value;
				if (collector.CanAutoDestroy())
					_removeList.Add(collector);
			}

			// 移除并销毁
			foreach (var collector in _removeList)
			{
				_collectors.Remove(collector.Location);
				collector.Destroy();
			}
		}
		void IModule.OnDestroy()
		{
			DestroyAll();
			DestroySingleton();
		}
		void IModule.OnGUI()
		{
		}

		/// <summary>
		/// 创建指定资源的游戏对象池
		/// </summary>
		/// <param name="location">资源定位地址</param>
		/// <param name="dontDestroy">是否常驻不销毁</param>
		/// <param name="initCapacity">初始的容器值</param>
		/// <param name="maxCapacity">最大的容器值</param>
		/// <param name="destroyTime">静默销毁时间（注意：小于零代表不主动销毁）</param>
		public GameObjectCollector CreatePool(string location, bool dontDestroy = false, int initCapacity = 0, int maxCapacity = int.MaxValue, float destroyTime = -1f)
		{
			if (_collectors.ContainsKey(location))
			{
				MotionLog.Warning($"Asset is already existed : {location}");
				return _collectors[location];
			}
			return CreatePoolInternal(location, dontDestroy, initCapacity, maxCapacity, destroyTime);
		}

		/// <summary>
		/// 是否都已经加载完毕
		/// </summary>
		public bool IsAllDone()
		{
			foreach (var pair in _collectors)
			{
				if (pair.Value.IsDone == false)
					return false;
			}
			return true;
		}

		/// <summary>
		/// 销毁所有对象池及其资源
		/// </summary>
		public void DestroyAll()
		{
			List<GameObjectCollector> removeList = new List<GameObjectCollector>();
			foreach (var pair in _collectors)
			{
				if (pair.Value.DontDestroy == false)
					removeList.Add(pair.Value);
			}

			// 移除并销毁
			foreach (var collector in removeList)
			{
				_collectors.Remove(collector.Location);
				collector.Destroy();
			}

			MotionLog.Log("Destroy all GameObjectPool !");
		}

		/// <summary>
		/// 获取游戏对象
		/// </summary>
		/// <param name="location">资源定位地址</param>
		/// <param name="forceClone">强制克隆游戏对象，忽略缓存池里的对象</param>
		/// <param name="userData">用户自定义数据</param>
		public SpawnGameObject Spawn(string location, bool forceClone = false, params System.Object[] userDatas)
		{
			if (_collectors.ContainsKey(location))
			{
				return _collectors[location].Spawn(forceClone, userDatas);
			}
			else
			{
				// 如果不存在创建游戏对象池
				GameObjectCollector pool = CreatePoolInternal(location, false, _defaultInitCapacity, _defaultMaxCapacity, _defaultDestroyTime);
				return pool.Spawn(forceClone, userDatas);
			}
		}

		private GameObjectCollector CreatePoolInternal(string location, bool dontDestroy, int initCapacity, int maxCapacity, float destroyTime)
		{
			GameObjectCollector pool = new GameObjectCollector(_root.transform, location, dontDestroy, initCapacity, maxCapacity, destroyTime);
			_collectors.Add(location, pool);
			return pool;
		}

		#region 调试专属方法
		internal Dictionary<string, GameObjectCollector> GetAllCollectors
		{
			get { return _collectors; }
		}
		#endregion
	}
}