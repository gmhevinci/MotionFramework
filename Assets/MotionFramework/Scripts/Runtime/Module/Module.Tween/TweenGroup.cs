//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Tween
{
	public class TweenGroup
	{
		private readonly List<ITweenNode> _cachedNodes = new List<ITweenNode>(100);

		/// <summary>
		/// 添加一个节点
		/// </summary>
		public void AddNode(ITweenNode node)
		{
			if (_cachedNodes.Contains(node))
				return;

			_cachedNodes.Add(node);
			TweenManager.Instance.Add(node);
		}

		/// <summary>
		/// 移除所有缓存的节点
		/// </summary>
		public void RemoveAllNodes()
		{
			for(int i=0; i<_cachedNodes.Count; i++)
			{
				TweenManager.Instance.Remove(_cachedNodes[i]);
			}
			_cachedNodes.Clear();
		}
	}
}