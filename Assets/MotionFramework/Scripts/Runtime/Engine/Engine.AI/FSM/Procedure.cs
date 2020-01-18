//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.AI
{
	/// <summary>
	/// 流程
	/// </summary>
	public class Procedure
	{
		private readonly List<string> _nodeNames = new List<string>();
		private readonly FiniteStateMachine _fsm = new FiniteStateMachine();

		/// <summary>
		/// 当前运行的节点名称
		/// </summary>
		public string Current
		{
			get
			{
				return _fsm.CurrentNodeName;
			}
		}

		/// <summary>
		/// 之前运行的节点名称
		/// </summary>
		public string Previous
		{
			get
			{
				return _fsm.PreviousNodeName;
			}
		}

		/// <summary>
		/// 添加一个流程节点
		/// 注意：流程节点会按照添加的先后顺序执行
		/// </summary>
		public void AddNode(IFsmNode node)
		{
			_fsm.AddNode(node);
			if (_nodeNames.Contains(node.Name) == false)
				_nodeNames.Add(node.Name);
		}

		/// <summary>
		/// 运行流程系统
		/// </summary>
		public void Run()
		{
			if (_nodeNames.Count > 0)
				_fsm.Run(_nodeNames[0]);
			else
				MotionLog.Log(ELogLevel.Warning, "Procedure system dont has any node.");
		}

		/// <summary>
		/// 更新流程系统
		/// </summary>
		public void Update()
		{
			_fsm.Update();
		}

		/// <summary>
		/// 切换流程节点
		/// </summary>
		public void Switch(string nodeName)
		{
			_fsm.Transition(nodeName);
		}

		/// <summary>
		/// 切换至下个流程节点
		/// </summary>
		public void SwitchNext()
		{
			int index = _nodeNames.IndexOf(_fsm.CurrentNodeName);
			if (index >= _nodeNames.Count - 1)
			{
				MotionLog.Log(ELogLevel.Warning, $"Current node {_fsm.CurrentNodeName} is end node.");
			}
			else
			{
				Switch(_nodeNames[index + 1]);
			}
		}

		/// <summary>
		/// 切换至上个流程节点
		/// </summary>
		public void SwitchLast()
		{
			int index = _nodeNames.IndexOf(_fsm.CurrentNodeName);
			if (index <= 0)
			{
				MotionLog.Log(ELogLevel.Warning, $"Current node {_fsm.CurrentNodeName} is begin node.");
			}
			else
			{
				Switch(_nodeNames[index - 1]);
			}
		}
	}
}