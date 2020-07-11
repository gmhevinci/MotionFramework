//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Tween;

namespace UnityEngine.UI
{
    public static class UnityEngine_UI_Image_Tween_Extension
    {
        public static ColorTween TweenColor(this Image obj, float duration, Color from, Color to)
        {
            ColorTween node = ColorTween.Allocate(duration, from, to);
            node.SetUpdate((result) => { obj.color = result; });
            return node;
        }
        public static ColorTween TweenColorTo(this Image obj, float duration, Color to)
        {
            return TweenColor(obj, duration, obj.color, to);
        }
        public static ColorTween TweenColorFrom(this Image obj, float duration, Color from)
        {
            return TweenColor(obj, duration, from, obj.color);
        }
    }
}
