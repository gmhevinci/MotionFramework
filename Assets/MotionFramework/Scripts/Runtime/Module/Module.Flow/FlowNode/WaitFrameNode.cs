//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Flow
{
	/// <summary>
	/// 帧数等待节点
	/// </summary>
	public class WaitFrameNode: IFlowNode
	{
		public static WaitFrameNode Allocate(int waitFrame)
		{
			WaitFrameNode node = new WaitFrameNode
			{
				WaitFrame = waitFrame,
			};
			return node;
		}

		private int _framer = 0;

		public bool IsDone { private set; get; } = false;
		public int WaitFrame { set; get; }

		void IFlowNode.OnUpdate()
		{
			_framer++;
			IsDone = _framer > WaitFrame;
		}
		void IFlowNode.OnDispose()
		{
		}
	}
}