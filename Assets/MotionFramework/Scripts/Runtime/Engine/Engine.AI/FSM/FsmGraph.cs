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
	/// <summary>
	/// 转换关系图
	/// </summary>
	public class FsmGraph
	{
		private readonly Dictionary<string, List<string>> _graph = new Dictionary<string, List<string>>();
		private readonly string _globalNode;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="globalNode">全局节点不受转换关系的限制</param>
		public FsmGraph(string globalNode)
		{
			_globalNode = globalNode;
		}

		/// <summary>
		/// 添加转换关系
		/// </summary>
		/// <param name="nodeName">节点名称</param>
		/// <param name="transitionNodes">可以转换到的节点列表</param>
		public void AddTransition(string nodeName, List<string> transitionNodes)
		{
			if (transitionNodes == null)
				throw new ArgumentNullException();

			if (_graph.ContainsKey(nodeName))
			{
				MotionLog.Log(ELogLevel.Warning, $"Graph node {nodeName} already existed.");
				return;
			}

			_graph.Add(nodeName, transitionNodes);
		}

		/// <summary>
		/// 检测转换关系
		/// </summary>
		public bool CanTransition(string from, string to)
		{
			if (_graph.ContainsKey(from) == false)
			{
				MotionLog.Log(ELogLevel.Warning, $"Not found graph node {from}");
				return false;
			}

			if (to == _globalNode)
				return true;

			return _graph[from].Contains(to);
		}
	}
}