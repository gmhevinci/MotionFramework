//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Flow
{
	public class FlowGroup
	{
		private readonly List<IFlowNode> _cachedNodes = new List<IFlowNode>(100);

		/// <summary>
		/// 添加一个节点
		/// </summary>
		public void AddNode(IFlowNode node)
		{
			if (_cachedNodes.Contains(node))
				return;

			_cachedNodes.Add(node);
			FlowManager.Instance.Add(node);
		}

		/// <summary>
		/// 移除所有缓存的节点
		/// </summary>
		public void RemoveAllNodes()
		{
			for(int i=0; i<_cachedNodes.Count; i++)
			{
				FlowManager.Instance.Remove(_cachedNodes[i]);
			}
			_cachedNodes.Clear();
		}
	}
}