//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Flow
{
	/// <summary>
	/// 顺序执行的复合节点
	/// 说明：节点列表依次执行，每个子节点结束之后便执行下一个节点，所有节点都结束时复合节点结束。
	/// </summary>
	public class SequenceNode : IFlowNode
	{
		public static SequenceNode Allocate(params IFlowNode[] nodes)
		{
			SequenceNode sequence = new SequenceNode();
			sequence.AddNode(nodes);	
			return sequence;
		}

		protected List<IFlowNode> _nodes = new List<IFlowNode>();
		protected IFlowNode _currentNode;

		public bool IsDone { private set; get; } = false;

		public IFlowNode CurrentNode
		{
			get { return _currentNode; }
		}

		public void AddNode(IFlowNode node)
		{
			if (_nodes.Contains(node) == false)
				_nodes.Add(node);
		}

		public void AddNode(params IFlowNode[] nodes)
		{
			foreach (var node in nodes)
			{
				AddNode(node);
			}
		}

		void IFlowNode.OnUpdate()
		{
			bool isAllDone = false;
			for (int index = 0; index < _nodes.Count; index++)
			{
				_currentNode = _nodes[index];
				if (_currentNode.IsDone)
					continue;

				_currentNode.OnUpdate();
				if (_currentNode.IsDone == false)
					break;

				if (index >= _nodes.Count - 1)
					isAllDone = true;
			}
			IsDone = isAllDone;
		}
		void IFlowNode.OnDispose()
		{
		}
	}
}