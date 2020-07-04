//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using MotionFramework.Flow;

namespace MotionFramework.Tween
{
	/*
	public class TweenNode<T> : IFlowNode
	{
		public static TweenNode<T> Allocate(float duration, System.Action<TweenNode<T>> progress)
		{
			return new TweenNode<T>(duration, progress);
		}

		private readonly float _duration;
		private readonly System.Action<TweenNode<T>> _progress;
		private bool _ignoreTimeScale = false;
		private TweenEaseDelegate _ease;
		private float _timer = 0;

		public TweenNode(float duration, System.Action<TweenNode<T>> progress)
		{
			_duration = duration;
			_progress = progress;
			_ease = Linear.Ease;
		}

		public bool IsDone { private set; get; } = false;

		void IFlowNode.OnDispose()
		{
		}
		void IFlowNode.OnUpdate()
		{
			float delatTime = _ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			_timer += delatTime;
			if (_duration > 0 && _timer < _duration)
			{
				float rate = _timer / _duration;
				_progress(this);
			}
			else
			{
				_progress(this);
				IsDone = true;
			}
		}

		public TweenNode<T> IgnoreTimeScale(bool value)
		{
			_ignoreTimeScale = value;
			return this;
		}
		public TweenNode<T> SetEase(TweenEaseDelegate ease)
		{
			_ease = ease;
			return this;
		}
	}
	*/
}