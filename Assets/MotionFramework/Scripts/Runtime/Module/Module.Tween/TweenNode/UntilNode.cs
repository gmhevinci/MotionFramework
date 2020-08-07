//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	/// <summary>
	/// 条件等待节点
	/// </summary>
	public class UntilNode : ITweenNode
	{
		public static UntilNode Allocate(System.Func<bool> condition)
		{
			UntilNode node = new UntilNode
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