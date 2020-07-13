//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Utility;
using UnityEngine;

namespace MotionFramework.Tween
{
	/// <summary>
	/// 计时器节点
	/// </summary>
	public class TimerNode : ITweenNode
	{
		/// <summary>
		/// 延迟计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="triggerCallback">触发事件</param>
		public static TimerNode AllocateDelay(float delay, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreateOnceTimer(delay);
			return new TimerNode(timer, triggerCallback);
		}

		/// <summary>
		/// 重复计时节点
		/// 注意：该节点为无限时长
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="triggerCallback">触发事件</param>
		public static TimerNode AllocateRepeat(float delay, float interval, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval);
			return new TimerNode(timer, triggerCallback);
		}

		/// <summary>
		/// 重复计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="duration">持续时间</param>
		/// <param name="triggerCallback">触发事件</param>
		/// <returns></returns>
		public static TimerNode AllocateRepeat(float delay, float interval, float duration, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval, duration);
			return new TimerNode(timer, triggerCallback);
		}

		/// <summary>
		/// 重复计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="maxTriggerCount">最大触发次数</param>
		/// <param name="triggerCallback">触发事件</param>
		/// <returns></returns>
		public static TimerNode AllocateRepeat(float delay, float interval, long maxTriggerCount, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval, maxTriggerCount);
			return new TimerNode(timer, triggerCallback);
		}

		/// <summary>
		/// 持续计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="duration">持续时间</param>
		/// <param name="triggerCallback">触发事件</param>
		/// <returns></returns>
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
		void ITweenNode.OnUpdate()
		{
			float delatTime = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			if (_timer.Update(delatTime))
			{
				_triggerCallback?.Invoke();
			}
			IsDone = _timer.IsOver;
		}
		void ITweenNode.OnDispose()
		{
		}
		void ITweenNode.Kill()
		{
			IsDone = true;
		}
	}
}