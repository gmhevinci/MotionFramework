//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.AI
{
	/// <summary>
	/// 启发式方法
	/// </summary>
	public static class AStarHeuristic
	{
		private static readonly float Sqrt2 = Mathf.Sqrt(2f);

		/// <summary>
		/// 曼哈顿距离
		/// 适合四方向(上下左右)移动
		/// </summary>
		public static float ManhattanDist(Vector3 a, Vector3 b)
		{
			return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
		}

		/// <summary>
		///  切比雪夫距离
		///  适合八方向(包括斜对角)移动
		/// </summary>
		public static float ChebyshevDist(Vector3 a, Vector3 b)
		{
			// Diagonal distance = D * max(dx, dy) + (D2 - D) * min(dx, dy)
			// D2 = 1, D = 1 ----> max(dx, dy)

			float dx = Mathf.Abs(a.x - b.x);
			float dy = Mathf.Abs(a.y - b.y);
			return Mathf.Max(dx, dy);
		}

		/// <summary>
		/// Octile距离
		/// 适合八方向(包括斜对角)移动
		/// </summary>
		public static float OctileDist(Vector3 a, Vector3 b)
		{
			// Diagonal distance = D * max(dx, dy) + (D2 - D) * min(dx, dy)
			// D2 = sqrt(2), D = 1 ---> max(dx, dy) + (Sqrt2 - 1) * min(dx, dy)

			float dx = Mathf.Abs(a.x - b.x);
			float dy = Mathf.Abs(a.y - b.y);
			return Mathf.Max(dx, dy) + (Sqrt2 - 1f) * Mathf.Min(dx, dy);
		}

		/// <summary>
		/// 欧式距离
		/// 适合任意方向移动
		/// </summary>
		public static float EuclideanDist(Vector3 a, Vector3 b)
		{
			return Vector3.Distance(a, b);
		}
	}
}