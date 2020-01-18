//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Console;
using MotionFramework.Reference;

namespace MotionFramework.Event
{
	/// <summary>
	/// 事件管理器
	/// </summary>
	public sealed class EventManager : ModuleSingleton<EventManager>, IModule
	{
		private readonly Dictionary<int, List<Action<IEventMessage>>> _listeners = new Dictionary<int, List<Action<IEventMessage>>>();

		void IModule.OnCreate(System.Object param)
		{
		}
		void IModule.OnUpdate()
		{
		}
		void IModule.OnGUI()
		{
			ConsoleGUI.Lable($"[{nameof(EventManager)}] Listener total count : {GetAllListenerCount()}");
		}

		/// <summary>
		/// 添加监听
		/// </summary>
		public void AddListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
		{
			int eventId = typeof(TEvent).GetHashCode();
			if (_listeners.ContainsKey(eventId) == false)
				_listeners.Add(eventId, new List<Action<IEventMessage>>());
			if (_listeners[eventId].Contains(listener) == false)
				_listeners[eventId].Add(listener);
		}

		/// <summary>
		/// 移除监听
		/// </summary>
		public void RemoveListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
		{
			int eventId = typeof(TEvent).GetHashCode();
			if (_listeners.ContainsKey(eventId))
			{
				if (_listeners[eventId].Contains(listener))
					_listeners[eventId].Remove(listener);
			}
		}

		/// <summary>
		/// 广播事件
		/// </summary>
		public void SendMessage(IEventMessage message)
		{
			int eventId = message.GetType().GetHashCode();
			if (_listeners.ContainsKey(eventId) == false)
				return;

			List<Action<IEventMessage>> listeners = _listeners[eventId];
			for (int i = 0; i < listeners.Count; i++)
			{
				listeners[i].Invoke(message);
			}

			// 回收引用对象
			IReference refClass = message as IReference;
			if (refClass != null)
				ReferencePool.Release(refClass);
		}

		/// <summary>
		/// 清空所有监听
		/// </summary>
		public void ClearListeners()
		{
			foreach (int eventId in _listeners.Keys)
			{
				_listeners[eventId].Clear();
			}
			_listeners.Clear();
		}

		/// <summary>
		/// 获取监听者总数
		/// </summary>
		private int GetAllListenerCount()
		{
			int count = 0;
			foreach (var list in _listeners)
			{
				count += list.Value.Count;
			}
			return count;
		}
	}
}