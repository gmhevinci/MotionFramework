//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 Zens
// Copyright©2020-2020 何冠峰 
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Tween
{
	public sealed class TweenScale : TransformNode
	{
		public Vector3 From;
		public Vector3 To;

		public static TweenScale AllocateTo(Transform target, float duration, Vector3 to)
		{
			TweenScale node = new TweenScale
			{
				Target = target,
				Duration = duration,
				From = target.localScale,
				To = to,
			};
			return node;
		}

		public static TweenScale Allocate(Transform target, float duration, Vector3 speed)
		{
			TweenScale node = new TweenScale
			{
				Target = target,
				Duration = duration,
				From = target.localScale,
				To = target.localScale + speed * duration,
			};
			return node;
		}

		protected override void Update(float progress)
		{
			Target.localScale = Vector3.Lerp(From, To, progress);
		}
	}
}