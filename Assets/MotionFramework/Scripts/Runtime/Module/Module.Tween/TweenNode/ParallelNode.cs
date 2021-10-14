//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	/// <summary>
	/// 并行执行的复合节点
	/// 说明：节点列表并行执行，所有子节点同时执行，所有节点都结束时复合节点结束。
	/// </summary>
	public class ParallelNode : ChainNode
	{
		protected override void UpdateChain(float deltaTime)
		{
			bool isAllDone = true;
			for (int index = 0; index < _nodes.Count; index++)
			{
				var node = _nodes[index];
				if (node.IsDone)
					continue;

				node.OnUpdate(deltaTime);
				if (node.IsDone == false)
				{
					isAllDone = false;
					continue;
				}
			}
			IsDone = isAllDone;
		}
	}
}