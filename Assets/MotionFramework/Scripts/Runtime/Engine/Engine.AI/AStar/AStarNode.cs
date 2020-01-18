//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.AI
{
	public abstract class AStarNode
	{
		/// <summary>
		/// 是否为阻挡节点
		/// </summary>
		public bool Block { set; get; }


		/// <summary>
		/// 总的代价值
		/// </summary>
		internal float Cost
		{
			get
			{
				return G + H;
			}
		}

		/// <summary>
		/// 路径搜索的时候，临时设立的G值
		/// 从起点移动到该节点的代价
		/// </summary>
		internal float G { set; get; }

		/// <summary>
		/// 路径搜索的时候，临时设立的H值
		/// 从该节点移动到终点的代价
		/// </summary>
		internal float H { set; get; }

		/// <summary>
		/// 路径搜索的时候，临时设立的父类节点
		/// </summary>
		internal AStarNode Parent { set; get; }

		/// <summary>
		/// 清空临时数据
		/// </summary>
		public void ClearTemp()
		{
			G = 0;
			H = 0;
			Parent = null;
		}
	}
}