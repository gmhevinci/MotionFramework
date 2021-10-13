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
        public static ColorTween TweenColor(this Text obj, float duration, Color from, Color to)
        {
            ColorTween node = ColorTween.Allocate(duration, from, to);
            node.SetUpdate((result) => { obj.color = result; });
            return node;
        }
        public static ColorTween TweenColorTo(this Text obj, float duration, Color to)
		{
            return TweenColor(obj, duration, obj.color, to);
		}
        public static ColorTween TweenColorFrom(this Text obj, float duration, Color from)
		{
            return TweenColor(obj, duration, from, obj.color);
        }
    }
}
