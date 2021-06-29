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
	public static class AStarPathFinding
	{
		private static readonly List<AStarNode> _openList = new List<AStarNode>(1000);
		private static readonly HashSet<AStarNode> _closedList = new HashSet<AStarNode>();

		/// <summary>
		/// 获取一条路径
		/// </summary>
		/// <param name="graph">节点关系图</param>
		/// <param name="from">起点</param>
		/// <param name="to">终点</param>
		/// <returns>如果没有找到路径返回NULL</returns>
		public static List<AStarNode> FindPath(IAStarGraph graph, AStarNode from, AStarNode to)
		{
			// 清空上次寻路数据
			graph.ClearTemper();
			_openList.Clear();
			_closedList.Clear();

			// 开始寻找路径
			_openList.Add(from);
			while (_openList.Count > 0)
			{
				// 获取当前代价值最小的节点
				AStarNode current = _openList[0];
				for (int i = 1; i < _openList.Count; i++)
				{
					if (_openList[i].Cost < current.Cost)
						current = _openList[i];
				}

				_openList.Remove(current);
				_closedList.Add(current);

				// 成功找到终点
				if (current == to)
				{
					return RetracePath(from, to);
				}

				// 获取邻居节点并添加到开放列表
				foreach (AStarNode neighbor in graph.Neighbors(current))
				{
					if (neighbor == null)
						continue;

					// 如果是障碍物
					if (neighbor.IsBlock())
						continue;

					// 如果已经在封闭列表里
					if (_closedList.Contains(neighbor))
						continue;

					float newCostToNeighbor = current.G + graph.CalculateCost(current, neighbor);
					if (_openList.Contains(neighbor) == false)
					{
						_openList.Add(neighbor);
						neighbor.G = newCostToNeighbor;
						neighbor.H = graph.CalculateCost(neighbor, to);
						neighbor.Parent = current;
						continue;
					}
					if (newCostToNeighbor < neighbor.G)
					{
						neighbor.G = newCostToNeighbor;
						neighbor.H = graph.CalculateCost(neighbor, to);
						neighbor.Parent = current;
					}
				}
			}

			// 没有找到路径
			return null;
		}

		/// <summary>
		/// 回溯路径
		/// </summary>
		private static List<AStarNode> RetracePath(AStarNode from, AStarNode to)
		{
			List<AStarNode> path = new List<AStarNode>();
			AStarNode current = to;
			while (current != from)
			{
				path.Add(current);
				current = current.Parent;
			}
			path.Reverse();
			return path;
		}
	}
}