//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Flow
{
	/// <summary>
	/// 时间等待节点
	/// </summary>
	public class WaitSecondFlow: IFlowNode
	{
		public static WaitSecondFlow Allocate(float waitTime, bool ignoreTimeScale = false)
		{
			WaitSecondFlow node = new WaitSecondFlow
			{
				WaitTime = waitTime,
				IgnoreTimeScale = ignoreTimeScale
			};
			return node;
		}

		private float _timer = 0;

		public bool IsDone { private set; get; } = false;
		public float WaitTime { set; get; }
		public bool IgnoreTimeScale { set; get; }

		void IFlowNode.OnUpdate()
		{
			float delatTime = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			_timer += delatTime;
			IsDone = _timer >= WaitTime;
		}
		void IFlowNode.OnDispose()
		{
		}
	}
}