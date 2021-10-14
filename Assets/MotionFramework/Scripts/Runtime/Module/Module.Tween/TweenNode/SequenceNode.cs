//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	/// <summary>
	/// 顺序执行的复合节点
	/// 说明：节点列表依次执行，每个子节点结束之后便执行下一个节点，所有节点都结束时复合节点结束。
	/// </summary>
	public class SequenceNode : ChainNode
	{
		protected ITweenNode _currentNode;
		public ITweenNode CurrentNode
		{
			get { return _currentNode; }
		}

		protected override void UpdateChain(float deltaTime)
		{
			bool isAllDone = false;
			for (int index = 0; index < _nodes.Count; index++)
			{
				_currentNode = _nodes[index];
				if (_currentNode.IsDone)
					continue;

				_currentNode.OnUpdate(deltaTime);
				if (_currentNode.IsDone == false)
					break;

				if (index >= _nodes.Count - 1)
					isAllDone = true;
			}
			IsDone = isAllDone;
		}
	}
}