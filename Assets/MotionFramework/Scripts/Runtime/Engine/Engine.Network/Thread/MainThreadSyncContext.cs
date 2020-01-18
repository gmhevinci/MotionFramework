//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;

namespace MotionFramework.Network
{
	/// <summary>
	/// 注意：Unity3D中需要设置Scripting Runtime Version为.NET4.6
	/// </summary>
	internal sealed class MainThreadSyncContext : SynchronizationContext
	{
		public readonly static MainThreadSyncContext Instance = new MainThreadSyncContext();

		/// <summary>
		/// 线程同步队列
		/// </summary>
		private readonly ConcurrentQueue<Action> _safeQueue = new ConcurrentQueue<Action>();

		/// <summary>
		/// 同步其它线程里的回调到主线程里
		/// </summary>
		public void Update()
		{
			while (true)
			{
				Action action = null;
				if (_safeQueue.TryDequeue(out action) == false)
					return;
				action.Invoke();
			}
		}

		public override void Post(SendOrPostCallback callback, object state)
		{
			Action action = new Action(() => { callback(state); });
			_safeQueue.Enqueue(action);
		}
	}
}