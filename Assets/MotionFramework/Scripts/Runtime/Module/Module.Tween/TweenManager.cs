//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;
using MotionFramework.Console;

namespace MotionFramework.Tween
{
	/// <summary>
	/// 补间管理器
	/// </summary>
	public class TweenManager : ModuleSingleton<TweenManager>, IModule
	{
		private readonly List<ITweenNode> _nodes = new List<ITweenNode>(1000);
		private readonly List<ITweenNode> _temper = new List<ITweenNode>(1000);

		void IModule.OnCreate(object createParam)
		{
		}
		void IModule.OnUpdate()
		{
			_temper.Clear();

			// 注意：这里按照添加的先后顺序执行所有流程
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
				var node = _temper[i];		
				_nodes.Remove(node);
				node.OnDispose();
			}
		}
		void IModule.OnGUI()
		{
			ConsoleGUI.Lable($"[{nameof(TweenManager)}] Tween total count : {_nodes.Count}");
		}

		public void Add(ITweenNode node)
		{
			if (_nodes.Contains(node) == false)
				_nodes.Add(node);
		}
		public void Remove(ITweenNode node)
		{
			if(_nodes.Remove(node))
			{
				node.OnDispose();
			}
		}
	}
}