//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 Zens
// Copyright©2020-2020 何冠峰 
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Tween
{
	public sealed class TweenRotation : TransformNode
	{
		public Vector3 From;
		public Vector3 To;

		public static TweenRotation AllocateTo(Transform target, float duration, Vector3 to)
		{
			TweenRotation node = new TweenRotation
			{
				Target = target,
				Duration = duration,
				From = target.localEulerAngles,
				To = to,
			};
			return node;
		}

		public static TweenRotation Allocate(Transform target, float duration, Vector3 speed)
		{
			TweenRotation node = new TweenRotation
			{
				Target = target,
				Duration = duration,
				From = target.localEulerAngles,
				To = target.localEulerAngles + speed * duration,
			};
			return node;
		}

		protected override void Update(float progress)
		{
			Target.localEulerAngles = Vector3.Lerp(From, To, progress);
		}
	}
}