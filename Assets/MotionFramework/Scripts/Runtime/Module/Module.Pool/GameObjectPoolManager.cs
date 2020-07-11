//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionFramework.Pool
{
	/// <summary>
	/// 游戏对象池管理器
	/// </summary>
	public sealed class GameObjectPoolManager : ModuleSingleton<GameObjectPoolManager>, IModule
	{
		private readonly Dictionary<string, GameObjectCollector> _collectors = new Dictionary<string, GameObjectCollector>();
		private GameObject _root;


		void IModule.OnCreate(object createParam)
		{
			_root = new GameObject("[PoolManager]");
			_root.transform.position = Vector3.zero;
			_root.transform.eulerAngles = Vector3.zero;
			UnityEngine.Object.DontDestroyOnLoad(_root);
		}
		void IModule.OnUpdate()
		{
		}
		void IModule.OnGUI()
		{
		}

		/// <summary>
		/// 创建指定资源的游戏对象池
		/// </summary>
		public GameObjectCollector CreatePool(string location, int capacity = 0, bool dontDestroy = false)
		{
			if (_collectors.ContainsKey(location))
			{
				MotionLog.Warning($"Asset is already existed : {location}");
				return _collectors[location];
			}
			return CreatePoolInternal(location, capacity, dontDestroy);
		}
		private GameObjectCollector CreatePoolInternal(string location, int capacity, bool dontDestroy)
		{
			GameObjectCollector pool = new GameObjectCollector(_root.transform, location, capacity, dontDestroy);
			_collectors.Add(location, pool);
			return pool;
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
		}

		/// <summary>
		/// 获取游戏对象
		/// </summary>
		public SpawnGameObject Spawn(string location)
		{
			if (_collectors.ContainsKey(location))
			{
				return _collectors[location].Spawn();
			}
			else
			{
				// 如果不存在创建游戏对象池
				GameObjectCollector pool = CreatePoolInternal(location, 0, false);
				return pool.Spawn();
			}
		}

		#region 调试专属方法
		internal Dictionary<string, GameObjectCollector> GetAllCollectors
		{
			get { return _collectors; }
		}
		#endregion
	}
}