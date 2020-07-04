//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Utility;
using UnityEngine;

namespace MotionFramework.Flow
{
	/// <summary>
	/// 计时器节点
	/// </summary>
	public class TimerNode : IFlowNode
	{
		public static TimerNode AllocateDelay(float delay, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreateOnceTimer(delay);
			return new TimerNode(timer, triggerCallback);
		}
		public static TimerNode AllocateRepeat(float delay, float interval, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval);
			return new TimerNode(timer, triggerCallback);
		}
		public static TimerNode AllocateRepeat(float delay, float interval, float duration, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval, duration);
			return new TimerNode(timer, triggerCallback);
		}
		public static TimerNode AllocateRepeat(float delay, float interval, long maxTriggerCount, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval, maxTriggerCount);
			return new TimerNode(timer, triggerCallback);
		}
		public static TimerNode AllocateDuration(float delay, float duration, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreateDurationTimer(delay, duration);
			return new TimerNode(timer, triggerCallback);
		}

		private readonly Timer _timer;
		private readonly System.Action _triggerCallback;
		public bool IgnoreTimeScale { set; get; } = false;
		public bool IsDone { private set; get; } = false;

		public TimerNode(Timer timer, System.Action triggerCallback)
		{
			_timer = timer;
			_triggerCallback = triggerCallback;
		}
		void IFlowNode.OnUpdate()
		{
			float delatTime = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			if (_timer.Update(delatTime))
			{
				_triggerCallback?.Invoke();
			}
			IsDone = _timer.IsOver;
		}
		void IFlowNode.OnDispose()
		{
		}
	}
}