//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Flow
{
	/// <summary>
	/// 修饰节点
	/// 持续运行一段时间
	/// </summary>
	public class RepeateNode : IFlowNode
	{
		public static RepeateNode Allocate(IFlowNode node, float duration,  bool ignoreTimeScale = false)
		{
			RepeateNode action = new RepeateNode
			{
				Node = node,
				Duration = duration,
				IgnoreTimeScale = ignoreTimeScale,
			};
			return action;
		}

		public bool IsDone { private set; get; } = false;
		public IFlowNode Node { set; get; }
		public float Duration { set; get; }
		public bool IgnoreTimeScale { set; get; }
		private float _timer = 0;

		void IFlowNode.OnUpdate()
		{
			float delatTime = IgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			_timer += delatTime;
			if(_timer < Duration)
			{
				Node.OnUpdate();
			}
			else
			{
				IsDone = true;
			}
		}
		void IFlowNode.OnDispose()
		{
		}
	}
}