//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Flow
{
	/// <summary>
	/// 条件等待节点
	/// </summary>
	public class WaitUntilNode : IFlowNode
	{
		public static WaitUntilNode Allocate(System.Func<bool> condition)
		{
			WaitUntilNode node = new WaitUntilNode
			{
				Condition = condition,
			};
			return node;
		}

		public bool IsDone { private set; get; } = false;
		public System.Func<bool> Condition { set; get; }

		void IFlowNode.OnUpdate()
		{
			IsDone = Condition.Invoke();
		}
		void IFlowNode.OnDispose()
		{
		}
	}
}