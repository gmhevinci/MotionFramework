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
		private class PostWrapper : IReference
		{
			public int PostFrame;
			public int EventID;
			public IEventMessage Message;

			public void OnRelease()
			{
				PostFrame = 0;
				EventID = 0;
				Message = null;
			}
		}

		private readonly Dictionary<int, LinkedList<Action<IEventMessage>>> _listeners = new Dictionary<int, LinkedList<Action<IEventMessage>>>(1000);
		private readonly List<PostWrapper> _postWrappers = new List<PostWrapper>(1000);

		void IModule.OnCreate(System.Object param)
		{
		}
		void IModule.OnUpdate()
		{
			for (int i = _postWrappers.Count - 1; i >= 0; i--)
			{
				var wrapper = _postWrappers[i];
				if (UnityEngine.Time.frameCount > wrapper.PostFrame)
				{
					SendMessage(wrapper.EventID, wrapper.Message);
					_postWrappers.RemoveAt(i);
					ReferencePool.Release(wrapper);
				}
			}
		}
		void IModule.OnDestroy()
		{
			DestroySingleton();
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
			AddListener(typeof(TEvent), listener);
		}

		/// <summary>
		/// 添加监听
		/// </summary>
		public void AddListener(System.Type eventType, System.Action<IEventMessage> listener)
		{
			int eventId = eventType.GetHashCode();
			AddListener(eventId, listener);
		}

		/// <summary>
		/// 添加监听
		/// </summary>
		public void AddListener(int eventId, System.Action<IEventMessage> listener)
		{
			if (_listeners.ContainsKey(eventId) == false)
				_listeners.Add(eventId, new LinkedList<Action<IEventMessage>>());
			if (_listeners[eventId].Contains(listener) == false)
				_listeners[eventId].AddLast(listener);
		}


		/// <summary>
		/// 移除监听
		/// </summary>
		public void RemoveListener<TEvent>(System.Action<IEventMessage> listener) where TEvent : IEventMessage
		{
			RemoveListener(typeof(TEvent), listener);
		}

		/// <summary>
		/// 移除监听
		/// </summary>
		public void RemoveListener(System.Type eventType, System.Action<IEventMessage> listener)
		{
			int eventId = eventType.GetHashCode();
			RemoveListener(eventId, listener);
		}

		/// <summary>
		/// 移除监听
		/// </summary>
		public void RemoveListener(int eventId, System.Action<IEventMessage> listener)
		{
			if (_listeners.ContainsKey(eventId))
			{
				if (_listeners[eventId].Contains(listener))
					_listeners[eventId].Remove(listener);
			}
		}


		/// <summary>
		/// 实时广播事件
		/// </summary>
		public void SendMessage(IEventMessage message)
		{
			int eventId = message.GetType().GetHashCode();
			SendMessage(eventId, message);
		}

		/// <summary>
		/// 实时广播事件
		/// </summary>
		public void SendMessage(int eventId, IEventMessage message)
		{
			if (_listeners.ContainsKey(eventId) == false)
				return;

			LinkedList<Action<IEventMessage>> listeners = _listeners[eventId];
			if (listeners.Count > 0)
			{
				var currentNode = listeners.Last;
				while (currentNode != null)
				{
					currentNode.Value.Invoke(message);
					currentNode = currentNode.Previous;
				}
			}

			// 回收引用对象
			IReference refClass = message as IReference;
			if (refClass != null)
				ReferencePool.Release(refClass);
		}

		/// <summary>
		/// 延迟广播事件
		/// </summary>
		public void PostMessage(IEventMessage message)
		{
			int eventId = message.GetType().GetHashCode();
			PostMessage(eventId, message);
		}

		/// <summary>
		/// 延迟广播事件
		/// </summary>
		public void PostMessage(int eventId, IEventMessage message)
		{
			var wrapper = ReferencePool.Spawn<PostWrapper>();
			wrapper.PostFrame = UnityEngine.Time.frameCount;
			wrapper.EventID = eventId;
			wrapper.Message = message;
			_postWrappers.Add(wrapper);
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