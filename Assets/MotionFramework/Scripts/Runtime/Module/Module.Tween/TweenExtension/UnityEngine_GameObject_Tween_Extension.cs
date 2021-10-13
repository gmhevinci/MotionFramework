//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Tween;

namespace UnityEngine
{
    public static class UnityEngine_GameObject_Tween_Extension
    {
        public static long PlayTween(this GameObject go, ITweenNode tween)
        {
            return TweenManager.Instance.Play(tween, go);
        }
        public static long PlayTween(this GameObject go, ITweenChain tween)
        {
            return TweenManager.Instance.Play(tween, go);
        }
        public static long PlayTween(this GameObject go, ChainNode tween)
		{
            return TweenManager.Instance.Play(tween, go);
        }
    }
}
