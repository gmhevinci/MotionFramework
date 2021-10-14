//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Tween
{
	/// <summary>
	/// 复合节点基类
	/// </summary>
	public abstract class ChainNode : ITweenNode, ITweenChain
	{
		private System.Action _onDispose = null;
		protected List<ITweenNode> _nodes = new List<ITweenNode>();

		public bool IsDone { protected set; get; } = false;

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
		public ITweenChain SetDispose(System.Action onDispose)
		{
			_onDispose = onDispose;
			return this;
		}

		void ITweenNode.OnUpdate(float deltaTime)
		{
			UpdateChain(deltaTime);
		}
		void ITweenNode.OnDispose()
		{
			foreach (var node in _nodes)
			{
				node.OnDispose();
			}
			_nodes.Clear();
			_onDispose?.Invoke();
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

		protected abstract void UpdateChain(float deltaTime);
	}
}