//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Tween
{
	public class TweenGroup
	{
		private readonly List<long> _cachedTweens = new List<long>(100);
		
		/// <summary>
		/// 播放一个补间动画
		/// 注意：缓存列表不会自动移除
		/// </summary>
		public void Play(ITweenNode node, UnityEngine.Object safeObject = null)
		{
			long tweenUID = TweenManager.Instance.Play(node, safeObject);
			if(tweenUID > 0)
				_cachedTweens.Add(tweenUID);
		}

		/// <summary>
		/// 移除所有缓存的补间动画
		/// </summary>
		public void RemoveAll()
		{
			for (int i = 0; i < _cachedTweens.Count; i++)
			{
				TweenManager.Instance.Kill(_cachedTweens[i]);
			}
			_cachedTweens.Clear();
		}
	}
}