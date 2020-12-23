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
	/// 同步其它线程里的回调到主线程里
	/// 注意：Unity3D中需要设置Scripting Runtime Version为.NET4.6
	/// </summary>
	public sealed class MainThreadSyncContext : SynchronizationContext
	{
		/// <summary>
		/// 同步队列
		/// </summary>
		private readonly ConcurrentQueue<Action> _safeQueue = new ConcurrentQueue<Action>();

		public void Update()
		{
			while (true)
			{
				if (_safeQueue.TryDequeue(out Action action) == false)
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