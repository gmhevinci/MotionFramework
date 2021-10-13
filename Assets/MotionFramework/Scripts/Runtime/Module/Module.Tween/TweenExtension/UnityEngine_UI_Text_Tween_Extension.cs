//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Tween;

namespace UnityEngine.UI
{
    public static class UnityEngine_UI_Text_Tween_Extension
    {
        public static ColorTween TweenAlpha(this Text obj, float duration, Color from, Color to)
        {
            ColorTween node = ColorTween.Allocate(duration, from, to);
            node.SetUpdate((result) => { obj.color = result; });
            return node;
        }
        public static ColorTween TweenAlphaTo(this Text obj, float duration, Color to)
		{
            return TweenAlpha(obj, duration, obj.color, to);
		}
        public static ColorTween TweenAlphaFrom(this Text obj, float duration, Color from)
		{
            return TweenAlpha(obj, duration, from, obj.color);
        }
    }
}
