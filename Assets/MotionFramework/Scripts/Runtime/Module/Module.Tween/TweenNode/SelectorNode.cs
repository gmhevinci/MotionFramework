//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Tween
{
	/// <summary>
	/// 随机执行的复合节点
	/// 说明：节点列表随机执行，在随机节点结束后复合节点结束。
	/// </summary>
	public class SelectorNode : ITweenNode
	{
		protected List<ITweenNode> _nodes = new List<ITweenNode>();
		protected ITweenNode _selectNode;

		public bool IsDone { private set; get; } = false;

		public ITweenNode SelectNode
		{
			get { return _selectNode; }
		}

		public void AddNode(ITweenNode node)
		{
			if (_nodes.Contains(node) == false)
				_nodes.Add(node);
		}
		public void AddNode(params ITweenNode[] nodes)
		{
			foreach (var node in nodes)
			{
				AddNode(node);
			}
		}

		void ITweenNode.OnUpdate()
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
		void ITweenNode.OnDispose()
		{
			foreach (var node in _nodes)
			{
				node.OnDispose();
			}
			_nodes.Clear();
		}
	}
}