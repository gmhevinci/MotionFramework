//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
	public static partial class UnityEngine_Transform_Extension
	{
		private static readonly Queue<Transform> _childStack = new Queue<Transform>(1000);

		/// <summary>
		/// 获取第一个子物体
		/// </summary>
		/// <param name="findActiveObject">激活条件</param>
		public static Transform GetFirstChild(this Transform root, bool findActiveObject = true)
		{
			if (root == null || root.childCount == 0)
				return null;

			if(findActiveObject == false)
				return root.GetChild(0);

			for (int i = 0; i < root.childCount; i++)
			{
				Transform target = root.GetChild(i);
				if (target.gameObject.activeSelf)
					return target;
			}
			return null;
		}

		/// <summary>
		/// 获取最后一个子物体
		/// </summary>
		/// <param name="findActiveObject">激活条件</param>
		public static Transform GetLastChild(this Transform root, bool findActiveObject = true)
		{
			if (root == null || root.childCount == 0)
				return null;

			if (findActiveObject == false)
				return root.GetChild(root.childCount - 1);

			for (int i = root.childCount - 1; i >= 0; i--)
			{
				Transform target = root.GetChild(i);	
				if (target.gameObject.activeSelf)
					return target;
			}
			return null;
		}

		/// <summary>
		/// 广度优先搜索查找子物体
		/// </summary>
		public static Transform BFSearch(this Transform root, string childName)
		{
			if (root == null) return null;

			_childStack.Clear();
			_childStack.Enqueue(root);

			while (_childStack.Count != 0)
			{
				root = _childStack.Dequeue();
				for (int i = 0; i < root.childCount; i++)
				{
					Transform child = root.GetChild(i);
					if (child.name == childName)
					{
						return child;
					}
					else
					{
						if (child.childCount != 0)
							_childStack.Enqueue(child);
					}
				}
			}

			// 没有发现返回空
			return null;
		}
		
		/// <summary>
		/// 广度优先搜索查找子物体的组件
		/// </summary>
		public static T BFSearch<T>(this Transform root, string childName) where T : UnityEngine.Component
		{
			Transform trans = root.BFSearch(childName);
			if (trans == null)
				return null;

			return trans.GetComponent<T>();
		}
	}
}