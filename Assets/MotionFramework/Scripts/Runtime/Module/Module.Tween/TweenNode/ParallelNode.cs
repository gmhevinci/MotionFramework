//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Tween
{
	/// <summary>
	/// 并行执行的复合节点
	/// 说明：节点列表并行执行，所有子节点同时执行，所有节点都结束时复合节点结束。
	/// </summary>
	public class ParallelNode : ITweenNode, ITweenChain
	{
		public static ParallelNode Allocate(params ITweenNode[] nodes)
		{
			ParallelNode sequence = new ParallelNode();
			sequence.AddNode(nodes);
			return sequence;
		}

		protected List<ITweenNode> _nodes = new List<ITweenNode>();

		public bool IsDone { private set; get; } = false;

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
		void ITweenNode.OnDispose()
		{
			foreach(var node in _nodes)
			{
				node.OnDispose();
			}
			_nodes.Clear();
		}
		void ITweenNode.Kill()
		{
			IsDone = true;
		}

		ITweenChain ITweenChain.Append(ITweenNode node)
		{
			AddNode(node);
			return this;
		}
	}
}