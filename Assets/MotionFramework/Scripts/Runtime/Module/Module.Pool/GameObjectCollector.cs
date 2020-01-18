//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Resource;

namespace MotionFramework.Pool
{
	internal class GameObjectCollector
	{
		private readonly Queue<GameObject> _collector;
		private AssetReference _assetRef;
		private AssetOperationHandle _handle;
		private GameObject _go;
		private Transform _root;
		private Action<GameObject> _userCallback;

		/// <summary>
		/// 对象池容量
		/// </summary>
		public int Capacity { private set; get; }

		/// <summary>
		/// 资源定位地址
		/// </summary>
		public string Location
		{
			get
			{
				return _assetRef.Location;
			}
		}

		/// <summary>
		/// 是否加载完毕
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _handle.IsDone;
			}
		}

		/// <summary>
		/// 当前的加载状态
		/// </summary>
		public EAssetStates States
		{
			get
			{
				return _handle.States;
			}
		}

		/// <summary>
		/// 内部缓存总数
		/// </summary>
		public int Count
		{
			get { return _collector.Count; }
		}

		/// <summary>
		/// 外部使用总数
		/// </summary>
		public int SpawnCount { private set; get; }


		public GameObjectCollector(Transform root, string location, int capacity)
		{
			_root = root;
			Capacity = capacity;

			// 创建缓存池
			_collector = new Queue<GameObject>(capacity);

			// 加载资源
			_assetRef = new AssetReference(location);
			_handle = _assetRef.LoadAssetAsync<GameObject>();
			_handle.Completed += Handle_Completed;
		}
		private void Handle_Completed(AssetOperationHandle obj)
		{
			_go = _handle.InstantiateObject;

			// 如果加载失败，创建临时对象
			if (_go == null)
				_go = new GameObject(Location);

			// 设置游戏对象
			_go.SetActive(false);
			_go.transform.SetParent(_root);
			_go.transform.localPosition = Vector3.zero;

			// 创建初始对象
			for (int i = 0; i < Capacity; i++)
			{
				GameObject gameObject = GameObject.Instantiate(_go) as GameObject;
				RestoreInternal(gameObject);
			}

			// 最后返回结果
			if (_userCallback != null)
			{
				Delegate[] actions = _userCallback.GetInvocationList();
				for (int i = 0; i < actions.Length; i++)
				{
					var action = (Action<GameObject>)actions[i];
					Spawn(action);
				}
				_userCallback = null;
			}
		}

		/// <summary>
		/// 存储一个游戏对象
		/// </summary>
		public void Restore(GameObject go)
		{
			if (go == null)
				return;

			SpawnCount--;
			RestoreInternal(go);
		}
		private void RestoreInternal(GameObject go)
		{
			go.SetActive(false);
			go.transform.SetParent(_root);
			go.transform.localPosition = Vector3.zero;
			_collector.Enqueue(go);
		}

		/// <summary>
		/// 异步的方式获取一个游戏对象
		/// </summary>
		public void Spawn(Action<GameObject> callback)
		{
			if (IsDone == false)
			{
				_userCallback += callback;
				return;
			}

			if (_collector.Count > 0)
			{
				GameObject go = _collector.Dequeue();
				go.SetActive(true);
				go.transform.parent = null;
				callback.Invoke(go);
			}
			else
			{
				GameObject obj = GameObject.Instantiate(_go);
				obj.SetActive(true);
				callback.Invoke(obj);
			}
			SpawnCount++;
		}

		/// <summary>
		/// 同步的方式获取一个游戏对象
		/// </summary>
		public GameObject Spawn()
		{
			if (IsDone == false)
				throw new Exception($"{_assetRef.Location} is not done");

			GameObject go = null;
			if (_collector.Count > 0)
			{
				go = _collector.Dequeue();
				go.SetActive(true);
				go.transform.parent = null;
			}
			else
			{
				go = GameObject.Instantiate(_go);
				go.SetActive(true);
			}
			SpawnCount++;
			return go;
		}

		/// <summary>
		/// 销毁对象池
		/// </summary>
		public void Destroy()
		{
			// 卸载资源对象
			if (_assetRef != null)
			{
				_assetRef.Release();
				_assetRef = null;
			}

			// 销毁游戏对象
			foreach (var item in _collector)
			{
				GameObject.Destroy(item);
			}
			_collector.Clear();

			// 清空回调
			_userCallback = null;
			SpawnCount = 0;
		}
	}
}