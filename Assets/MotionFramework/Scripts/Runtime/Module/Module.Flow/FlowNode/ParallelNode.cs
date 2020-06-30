//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Flow
{
	/// <summary>
	/// 复合节点
	/// 节点列表并行执行，所有子节点同时执行，所有节点都结束时复合节点结束。
	/// </summary>
	public class ParallelFlow : IFlowNode
	{
		protected List<IFlowNode> _nodes = new List<IFlowNode>();

		public bool IsDone { private set; get; } = false;

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
			bool isAllDone = true;
			for (int index = 0; index < _nodes.Count; index++)
			{
				var node = _nodes[index];
				if (node.IsDone)
					continue;

				node.OnUpdate();
				if (node.IsDone == false)
				{
					isAllDone = false;
					continue;
				}
			}
			IsDone = isAllDone;
		}
		void IFlowNode.OnDispose()
		{
		}
	}
}