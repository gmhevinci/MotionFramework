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
		internal enum ESpawnState
		{
			None = 0,

			/// <summary>
			/// 已回收
			/// </summary>
			Restore,

			/// <summary>
			/// 已丢弃
			/// </summary>
			Discard,
		}


		private readonly GameObjectCollector _cacheCollector;

		/// <summary>
		/// 是否已经释放回收
		/// </summary>
		internal ESpawnState SpawnState { private set; get; } = ESpawnState.None;
		
		/// <summary>
		/// 游戏对象
		/// </summary>
		public GameObject Go { internal set; get; }

		/// <summary>
		/// 用户自定义数据集
		/// </summary>
		public System.Object[] UserDatas { private set; get; }

		/// <summary>
		/// 用户自定义数据
		/// </summary>
		public System.Object UserData 
		{ 
			get
			{
				if (UserDatas != null && UserDatas.Length > 0)
					return UserDatas[0];
				else
					return null;
			}
		}


		internal SpawnGameObject(GameObjectCollector collector, params System.Object[] userDatas)
		{
			_cacheCollector = collector;
			UserDatas = userDatas;
		}
		internal SpawnGameObject(GameObjectCollector collector, GameObject go, params System.Object[] userDatas)
		{
			_cacheCollector = collector;
			Go = go;
			UserDatas = userDatas;
		}

		/// <summary>
		/// 回收
		/// </summary>
		public void Restore()
		{
			UserCallback = null;
			SpawnState = ESpawnState.Restore;
			_cacheCollector.Restore(this);
		}

		/// <summary>
		/// 丢弃
		/// </summary>
		public void Discard()
		{
			UserCallback = null;
			SpawnState = ESpawnState.Discard;
			_cacheCollector.Discard(this);
		}

		#region 异步相关
		internal System.Action<SpawnGameObject> UserCallback;

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<SpawnGameObject> Completed
		{
			add
			{
				if (Go != null)
					value.Invoke(this);
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
