//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	/// <summary>
	/// 条件等待节点
	/// </summary>
	public class UntilNode : ITweenNode
	{
		public bool IsDone { private set; get; } = false;
		public System.Func<bool> Condition { set; get; }

		void ITweenNode.OnUpdate(float deltaTime)
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