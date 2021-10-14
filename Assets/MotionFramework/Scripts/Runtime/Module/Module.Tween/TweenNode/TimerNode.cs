//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Utility;

namespace MotionFramework.Tween
{
	/// <summary>
	/// 计时器节点
	/// </summary>
	public class TimerNode : ITweenNode
	{
		private readonly Timer _timer;
		private readonly System.Action _triggerCallback;
		public bool IsDone { private set; get; } = false;

		public TimerNode(Timer timer, System.Action triggerCallback)
		{
			_timer = timer;
			_triggerCallback = triggerCallback;
		}
		void ITweenNode.OnUpdate(float deltaTime)
		{
			if (_timer.Update(deltaTime))
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