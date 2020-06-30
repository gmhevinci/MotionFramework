//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 Zens
// Copyright©2020-2020 何冠峰 
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Tween
{
	public sealed class TweenMove : TransformNode
	{
		public Vector3 From;
		public Vector3 To;

		public static TweenMove AllocateTo(Transform target, float duration, Vector3 to)
		{
			TweenMove node = new TweenMove
			{
				Target = target,
				Duration = duration,
				From = target.localPosition,
				To = to,
			};
			return node;
		}

		public static TweenMove Allocate(Transform target, float duration, Vector3 speed)
		{
			TweenMove node = new TweenMove
			{
				Target = target,
				Duration = duration,
				From = target.localPosition,
				To = target.localPosition + speed * duration,
			};
			return node;
		}

		protected override void Update(float progress)
		{
			Target.localPosition = Vector3.Lerp(From, To, progress);
		}
	}
}