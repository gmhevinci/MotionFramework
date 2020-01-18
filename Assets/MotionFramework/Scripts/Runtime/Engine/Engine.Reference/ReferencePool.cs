//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Reference
{
	/// <summary>
	/// 引用池
	/// </summary>
	public static class ReferencePool
	{
		private static readonly Dictionary<Type, ReferenceCollector> _collectors = new Dictionary<Type, ReferenceCollector>();

		/// <summary>
		/// 对象池初始容量
		/// </summary>
		public static int InitCapacity { get; set; } = 100;

		/// <summary>
		/// 对象池的数量
		/// </summary>
		public static int Count
		{
			get
			{
				return _collectors.Count;
			}
		}


		/// <summary>
		/// 清除所有对象池
		/// </summary>
		public static void ClearAll()
		{
			foreach (KeyValuePair<Type, ReferenceCollector> pair in _collectors)
			{
				pair.Value.Clear();
			}
			_collectors.Clear();
		}

		/// <summary>
		/// 申请引用对象
		/// </summary>
		public static IReference Spawn(Type type)
		{
			if (_collectors.ContainsKey(type) == false)
			{
				_collectors.Add(type, new ReferenceCollector(type, InitCapacity));
			}
			return _collectors[type].Spawn();
		}

		/// <summary>
		/// 申请引用对象
		/// </summary>
		public static T Spawn<T>() where T : class, IReference, new()
		{
			Type type = typeof(T);
			return Spawn(type) as T;
		}

		/// <summary>
		/// 回收引用对象
		/// </summary>
		public static void Release(IReference item)
		{
			Type type = item.GetType();
			if (_collectors.ContainsKey(type) == false)
			{
				_collectors.Add(type, new ReferenceCollector(type, InitCapacity));
			}
			_collectors[type].Release(item);
		}

		/// <summary>
		/// 批量回收列表集合
		/// </summary>
		public static void Release<T>(List<T> items) where T : class, IReference, new()
		{
			Type type = typeof(T);
			if (_collectors.ContainsKey(type) == false)
			{
				_collectors.Add(type, new ReferenceCollector(type, InitCapacity));
			}

			for (int i = 0; i < items.Count; i++)
			{
				_collectors[type].Release(items[i]);
			}
		}

		/// <summary>
		/// 批量回收数组集合
		/// </summary>
		public static void Release<T>(T[] items) where T : class, IReference, new()
		{
			Type type = typeof(T);
			if (_collectors.ContainsKey(type) == false)
			{
				_collectors.Add(type, new ReferenceCollector(type, InitCapacity));
			}

			for (int i = 0; i < items.Length; i++)
			{
				_collectors[type].Release(items[i]);
			}
		}

		#region 调试专属方法
		internal static Dictionary<Type, ReferenceCollector> GetAllCollectors
		{
			get { return _collectors; }
		}
		#endregion
	}
}