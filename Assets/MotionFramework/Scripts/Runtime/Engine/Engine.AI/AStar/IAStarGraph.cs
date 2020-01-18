//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.AI
{
	public interface IAStarGraph
	{
		/// <summary>
		/// 获取邻居节点
		/// </summary>
		IEnumerable<AStarNode> Neighbors(AStarNode node);

		/// <summary>
		/// 计算移动代价
		/// </summary>
		float CalculateCost(AStarNode from, AStarNode to);

		/// <summary>
		/// 清空所有节点的临时数据
		/// </summary>
		void ClearTemp();
	}
}