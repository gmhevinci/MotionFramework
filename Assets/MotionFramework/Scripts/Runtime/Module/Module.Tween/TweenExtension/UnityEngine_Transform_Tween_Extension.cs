//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Tween;

namespace UnityEngine
{
    public static class UnityEngine_Transform_Tween_Extension
    {
		public static Vector3Tween TweenScale(this Transform obj, float duration, Vector3 from, Vector3 to)
        {
            Vector3Tween node = Vector3Tween.Allocate(duration, from, to);
            node.SetUpdate((result) => { obj.localScale = result; });
            return node;
        }
        public static Vector3Tween TweenScaleTo(this Transform obj, float duration, Vector3 to)
        {
            return TweenScale(obj, duration, obj.localScale, to);
        }
        public static Vector3Tween TweenScaleFrom(this Transform obj, float duration, Vector3 from)
        {
            return TweenScale(obj, duration, from, obj.localScale);
        }

        public static Vector3Tween ShakePosition(this Transform obj, float duration, Vector3 magnitude, bool relativeWorld = false)
		{
            Vector3 position = relativeWorld ? obj.position : obj.localPosition;
            Vector3Tween node = Vector3Tween.Allocate(duration, position, position);
            node.SetUpdate(
                (result) =>
                {
                    if (relativeWorld)
                        obj.position = result;
                    else
                        obj.localPosition = result;
                });
            node.SetLerp(
                (from, to, progress) =>
                {
                    return TweenMath.Shake(magnitude, from, progress);
                });
            node.SetLoop(ETweenLoop.PingPong, 1);
            return node;
        }
		public static Vector3Tween TweenPosition(this Transform obj, float duration, Vector3 from, Vector3 to, bool relativeWorld = false)
        {
            Vector3Tween node = Vector3Tween.Allocate(duration, from, to);
            node.SetUpdate(
                (result) => 
                {
                    if (relativeWorld)
                        obj.position = result;
                    else
                        obj.localPosition = result; 
                });
            return node;
        }
        public static Vector3Tween TweenPositionTo(this Transform obj, float duration, Vector3 to, bool relativeWorld = false)
        {
            Vector3 from = relativeWorld ? obj.position : obj.localPosition;
            return TweenPosition(obj, duration, from, to, relativeWorld);
        }
        public static Vector3Tween TweenPositionFrom(this Transform obj, float duration, Vector3 from, bool relativeWorld = false)
        {
            Vector3 to = relativeWorld ? obj.position : obj.localPosition;
            return TweenPosition(obj, duration, from, to, relativeWorld);
        }

        public static Vector3Tween TweenAngles(this Transform obj, float duration, Vector3 from, Vector3 to, bool relativeWorld = false)
        {
            Vector3Tween node = Vector3Tween.Allocate(duration, from, to);
            node.SetUpdate(
                (result) =>
                {
                    if (relativeWorld)
                        obj.eulerAngles = result;
                    else
                        obj.localEulerAngles = result;
                });
            return node;
        }
        public static Vector3Tween TweenAnglesTo(this Transform obj, float duration, Vector3 to, bool relativeWorld = false)
        {
            Vector3 from = relativeWorld ? obj.eulerAngles : obj.localEulerAngles;
            return TweenAngles(obj, duration, from, to, relativeWorld);
        }
        public static Vector3Tween TweenAnglesFrom(this Transform obj, float duration, Vector3 from, bool relativeWorld = false)
        {
            Vector3 to = relativeWorld ? obj.eulerAngles : obj.localEulerAngles;
            return TweenAngles(obj, duration, from, to, relativeWorld);
        }

        public static QuaternionTween TweenRotation(this Transform obj, float duration, Quaternion from, Quaternion to, bool relativeWorld = false)
        {
            QuaternionTween node = QuaternionTween.Allocate(duration, from, to);
            node.SetUpdate(
                (result) =>
                {
                    if (relativeWorld)
                        obj.rotation = result;
                    else
                        obj.localRotation = result;
                });
            return node;
        }
        public static QuaternionTween TweenRotationTo(this Transform obj, float duration, Quaternion to, bool relativeWorld = false)
        {
            Quaternion from = relativeWorld ? obj.rotation : obj.localRotation;
            return TweenRotation(obj, duration, from, to, relativeWorld);
        }
        public static QuaternionTween TweenRotationFrom(this Transform obj, float duration, Quaternion from, bool relativeWorld = false)
        {
            Quaternion to = relativeWorld ? obj.rotation : obj.localRotation;
            return TweenRotation(obj, duration, from, to, relativeWorld);
        }
    }
}