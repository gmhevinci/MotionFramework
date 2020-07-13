//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	/// <summary>
	/// 执行节点
	/// </summary>
	public class ExecuteNode : ITweenNode
	{
		public static ExecuteNode Allocate(System.Action execute)
		{
			ExecuteNode node = new ExecuteNode
			{
				Execute = execute,
			};
			return node;
		}

		public bool IsDone { private set; get; } = false;
		public System.Action Execute { set; get; }

		void ITweenNode.OnUpdate()
		{
			Execute.Invoke();
			IsDone = true;
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