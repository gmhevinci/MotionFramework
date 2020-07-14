//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	public class TweenGroup
	{
		/// <summary>
		/// 播放一个补间动画
		/// </summary>
		public long Play(ITweenNode node, UnityEngine.Object safeObject = null)
		{
			int groupID = this.GetHashCode();
			return TweenManager.Instance.Play(node, safeObject, groupID);
		}

		/// <summary>
		/// 关闭组内所有的补间动画
		/// </summary>
		public void KillAll()
		{
			int groupID = this.GetHashCode();
			TweenManager.Instance.Kill(groupID);
		}
	}
}