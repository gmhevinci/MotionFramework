//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 Zens
// Copyright©2020-2020 何冠峰 
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using MotionFramework.Flow;

namespace MotionFramework.Tween
{
	public abstract class TransformNode : IFlowNode
	{
		public Transform Target { get; set; }
		public float Duration { set; get; }
		public bool IgnoreTimeScale { set; get; }
		private float _timer = 0;

		public bool IsDone { private set; get; } = false;

		void IFlowNode.OnDispose()
		{
		}
		void IFlowNode.OnUpdate()
		{
			float delatTime = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			_timer += delatTime;
			if (Duration > 0 && _timer < Duration)
			{
				float progress = _timer / Duration;
				Update(progress);
			}
			else
			{
				Update(1f);
				IsDone = true;
			}
		}
		
		protected abstract void Update(float progress);
	}
}