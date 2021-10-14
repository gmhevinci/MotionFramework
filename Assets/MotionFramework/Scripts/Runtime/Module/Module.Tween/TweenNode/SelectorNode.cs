//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	/// <summary>
	/// 随机执行的复合节点
	/// 说明：节点列表随机执行，在随机节点结束后复合节点结束。
	/// </summary>
	public class SelectorNode : ChainNode
	{
		protected ITweenNode _selectNode;
		public ITweenNode SelectNode
		{
			get { return _selectNode; }
		}

		protected override void UpdateChain(float deltaTime)
		{
			if(_selectNode == null)
			{
				if (_nodes.Count > 0)
				{
					int index = UnityEngine.Random.Range(0, _nodes.Count);
					_selectNode = _nodes[index];
				}
				else
				{
					IsDone = true;
				}
			}

			if(_selectNode != null)
			{
				_selectNode.OnUpdate(deltaTime);
				IsDone = _selectNode.IsDone;
			}
		}
	}
}