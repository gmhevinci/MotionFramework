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
		public void CreatePool(string location, int capacity)
		{
			if (_collectors.ContainsKey(location))
			{
				MotionLog.Log(ELogLevel.Warning, $"Asset is already existed : {location}");
				return;
			}
			CreatePoolInternal(location, capacity);
		}
		private GameObjectCollector CreatePoolInternal(string location, int capacity)
		{
			GameObjectCollector pool = new GameObjectCollector(_root.transform, location, capacity);
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
			foreach (var pair in _collectors)
			{
				pair.Value.Destroy();
			}
			_collectors.Clear();
		}

		/// <summary>
		/// 异步方式获取一个游戏对象
		/// </summary>
		public void Spawn(string location, Action<GameObject> callbcak)
		{
			if (_collectors.ContainsKey(location))
			{
				_collectors[location].Spawn(callbcak);
			}
			else
			{
				// 如果不存在创建游戏对象池
				GameObjectCollector pool = CreatePoolInternal(location, 0);
				pool.Spawn(callbcak);
			}
		}

		/// <summary>
		/// 同步方式获取一个游戏对象
		/// </summary>
		public GameObject Spawn(string location)
		{
			if (_collectors.ContainsKey(location))
			{
				return _collectors[location].Spawn();
			}
			else
			{
				// 如果不存在创建游戏对象池
				GameObjectCollector pool = CreatePoolInternal(location, 0);
				return pool.Spawn();
			}
		}

		/// <summary>
		/// 回收一个游戏对象
		/// </summary>
		public void Restore(string location, GameObject obj)
		{
			if (obj == null)
				return;

			if (_collectors.ContainsKey(location))
			{
				_collectors[location].Restore(obj);
			}
			else
			{
				MotionLog.Log(ELogLevel.Error, $"GameObjectPool does not exist : {location}");
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