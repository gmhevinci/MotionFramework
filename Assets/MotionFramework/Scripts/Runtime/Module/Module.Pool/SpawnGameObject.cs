//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Pool
{
	public class SpawnGameObject
	{
		private GameObjectCollector _cacheCollector;

		/// <summary>
		/// 是否在使用中
		/// </summary>
		internal bool IsSpawning = false;
		
		/// <summary>
		/// 游戏对象
		/// </summary>
		public GameObject Go { internal set; get; }


		internal SpawnGameObject(GameObjectCollector collector)
		{
			_cacheCollector = collector;
		}
		internal SpawnGameObject(GameObjectCollector collector, GameObject go)
		{
			_cacheCollector = collector;
			Go = go;
		}

		/// <summary>
		/// 回收
		/// </summary>
		public void Restore()
		{
			UserCallback = null;
			_cacheCollector.Restore(this);
		}

		#region 异步相关
		internal System.Action<GameObject> UserCallback;

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<GameObject> Completed
		{
			add
			{
				if (Go != null)
					value.Invoke(Go);
				else
					UserCallback += value;
			}
			remove
			{
				UserCallback -= value;
			}
		}
		#endregion
	}
}
