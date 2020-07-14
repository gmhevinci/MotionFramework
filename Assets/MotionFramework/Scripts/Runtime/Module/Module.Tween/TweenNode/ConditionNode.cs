//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	/// <summary>
	/// 条件节点
	/// </summary>
	public class ConditionNode : ITweenNode
	{
		public static ConditionNode Allocate(System.Func<bool> condition)
		{
			ConditionNode node = new ConditionNode
			{
				Condition = condition,
			};
			return node;
		}

		public bool IsDone { private set; get; } = false;
		public System.Func<bool> Condition { set; get; }

		void ITweenNode.OnUpdate()
		{
			IsDone = Condition.Invoke();
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