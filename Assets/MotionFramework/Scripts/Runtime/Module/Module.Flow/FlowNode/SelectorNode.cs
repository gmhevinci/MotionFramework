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
	/// 节点列表随机执行，在随机节点结束后复合节点结束。
	/// </summary>
	public class SelectorNode : IFlowNode
	{
		protected List<IFlowNode> _nodes = new List<IFlowNode>();
		protected IFlowNode _selectNode;

		public bool IsDone { private set; get; } = false;

		public IFlowNode SelectNode
		{
			get { return _selectNode; }
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
			if(_selectNode == null)
			{
				if (_nodes.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, _nodes.Count);
					_selectNode = _nodes[index];
				}
				else
				{
					IsDone = true;
				}
			}

			if(_selectNode != null)
			{
				_selectNode.OnUpdate();
				IsDone = _selectNode.IsDone;
			}
		}
		void IFlowNode.OnDispose()
		{
		}
	}
}