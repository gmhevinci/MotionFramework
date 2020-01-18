//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.AI
{
	public static class AStarPatterns
	{
		/// <summary>
		/// 2D平面上的四方向
		/// </summary>
		public static readonly Vector3Int[] FourDirections =
		{
										new Vector3Int(0, 1, 0),
			new Vector3Int(-1, 0, 0),              new Vector3Int(1, 0, 0),
										new Vector3Int(0, -1, 0)
		};

		/// <summary>
		/// 2D平面上的八方向
		/// </summary>
		public static readonly Vector3Int[] EightDirections =
		{
			new Vector3Int(-1, 1, 0), new Vector3Int(0, 1, 0), new Vector3Int(1, 1, 0),
			new Vector3Int(-1, 0, 0),											new Vector3Int(1, 0, 0),
			new Vector3Int(-1, -1, 0), new Vector3Int(0, -1, 0), new Vector3Int(1, -1, 0)
		};

		/// <summary>
		/// 2D平面上的对角方向
		/// </summary>
		public static readonly Vector3Int[] DiagonalDirections =
		{
			new Vector3Int(-1, 1, 0),			new Vector3Int(1, 1, 0),

			new Vector3Int(-1, -1, 0),          new Vector3Int(1, -1, 0)
		};
	}
}