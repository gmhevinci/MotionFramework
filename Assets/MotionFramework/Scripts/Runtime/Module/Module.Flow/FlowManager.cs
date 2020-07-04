//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Flow
{
	/// <summary>
	/// 流程管理器
	/// </summary>
	public class FlowManager : ModuleSingleton<FlowManager>, IModule
	{
		private readonly List<IFlowNode> _nodes = new List<IFlowNode>(1000);
		private readonly List<IFlowNode> _temper = new List<IFlowNode>(1000);

		void IModule.OnCreate(object createParam)
		{
		}
		void IModule.OnUpdate()
		{
			_temper.Clear();
			for (int i=0; i<_nodes.Count; i++)
			{
				var node = _nodes[i];
				node.OnUpdate();
				if (node.IsDone)
					_temper.Add(node);
			}

			// 移除完成的节点
			for(int i=0; i<_temper.Count; i++)
			{
				_nodes.Remove(_temper[i]);
			}
		}
		void IModule.OnGUI()
		{
		}

		public void Add(IFlowNode node)
		{
			if (_nodes.Contains(node) == false)
				_nodes.Add(node);
		}
		public void Remove(IFlowNode node)
		{
			_nodes.Remove(node);
		}
	}
}