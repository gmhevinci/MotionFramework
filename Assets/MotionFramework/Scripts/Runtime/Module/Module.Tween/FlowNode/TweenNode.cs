//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Tween
{
	public abstract class TweenNode<T> : ITweenNode where T: struct
	{
		public delegate T TweenLerpDelegate(T from, T to, float progress);
		
		private readonly float _duration;
		private readonly T _from;
		private readonly T _to;

		private float _running = 0;
		private bool _ignoreTimeScale = false;
		private System.Action<T> _update = null;
		protected TweenEaseDelegate _easeFun = null;
		protected TweenLerpDelegate _lerpFun = null;

		/// <summary>
		/// 补间结果
		/// </summary>
		public T Result { get; private set; }

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone { private set; get; } = false;

		public TweenNode(float duration, T from, T to)
		{
			_duration = duration;
			_from = from;
			_to = to;
			_easeFun = TweenEase.Linear.Default;
		}
		void ITweenNode.OnDispose()
		{
		}
		void ITweenNode.OnUpdate()
		{
			float delatTime = _ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			_running += delatTime;
			if (_duration > 0 && _running < _duration)
			{
				float progress = _easeFun.Invoke(_running, 0, 1, _duration);
				Result = UpdateResultValue(_from, _to, progress);
				_update(Result);
			}
			else
			{
				Result = _to;
				_update(Result);
				IsDone = true;
			}
		}

		public TweenNode<T> IgnoreTimeScale(bool value)
		{
			_ignoreTimeScale = value;
			return this;
		}
		public TweenNode<T> SetEase(TweenEaseDelegate ease)
		{
			_easeFun = ease;
			return this;
		}
		public TweenNode<T> SetLerp(TweenLerpDelegate lerp)
		{
			_lerpFun = lerp;
			return this;
		}
		public TweenNode<T> SetUpdate(System.Action<T> update)
		{
			_update = update;
			return this;
		}

		protected abstract T UpdateResultValue(T from, T to, float progress);
	}

	/// <summary>
	/// FloatTween
	/// </summary>
	public sealed class FloatTween : TweenNode<float>
	{
		public static FloatTween Allocate(float duration, float from, float to)
		{
			return new FloatTween(duration, from, to);
		}

		public FloatTween(float duration, float from, float to) : base(duration, from, to)
		{
		}
		protected override float UpdateResultValue(float from, float to, float progress)
		{
			if (_lerpFun == null)
				return Mathf.Lerp(from, to, progress);
			else
				return _lerpFun.Invoke(from, to, progress);
		}
	}

	/// <summary>
	/// Vector2Tween
	/// </summary>
	public sealed class Vector2Tween : TweenNode<Vector2>
	{
		public static Vector2Tween Allocate(float duration, Vector2 from, Vector2 to)
		{
			return new Vector2Tween(duration, from, to);
		}

		public Vector2Tween(float duration, Vector2 from, Vector2 to) : base(duration, from, to)
		{
		}
		protected override Vector2 UpdateResultValue(Vector2 from, Vector2 to, float progress)
		{
			if (_lerpFun == null)
				return Vector2.Lerp(from, to, progress);
			else
				return _lerpFun.Invoke(from, to, progress);
		}
	}

	/// <summary>
	/// Vector3Tween
	/// </summary>
	public sealed class Vector3Tween : TweenNode<Vector3>
	{
		public static Vector3Tween Allocate(float duration, Vector3 from, Vector3 to)
		{
			return new Vector3Tween(duration, from, to);
		}

		public Vector3Tween(float duration, Vector3 from, Vector3 to) : base(duration, from, to)
		{
		}
		protected override Vector3 UpdateResultValue(Vector3 from, Vector3 to, float progress)
		{
			if (_lerpFun == null)
				return Vector3.Lerp(from, to, progress);
			else
				return _lerpFun.Invoke(from, to, progress);
		}
	}

	/// <summary>
	/// Vector4Tween
	/// </summary>
	public sealed class Vector4Tween : TweenNode<Vector4>
	{
		public static Vector4Tween Allocate(float duration, Vector4 from, Vector4 to)
		{
			return new Vector4Tween(duration, from, to);
		}

		public Vector4Tween(float duration, Vector4 from, Vector4 to) : base(duration, from, to)
		{
		}
		protected override Vector4 UpdateResultValue(Vector4 from, Vector4 to, float progress)
		{
			if (_lerpFun == null)
				return Vector4.Lerp(from, to, progress);
			else
				return _lerpFun.Invoke(from, to, progress);
		}
	}

	/// <summary>
	/// ColorTween
	/// </summary>
	public sealed class ColorTween : TweenNode<Color>
	{
		public static ColorTween Allocate(float duration, Color from, Color to)
		{
			return new ColorTween(duration, from, to);
		}

		public ColorTween(float duration, Color from, Color to) : base(duration, from, to)
		{
		}
		protected override Color UpdateResultValue(Color from, Color to, float progress)
		{
			if (_lerpFun == null)
				return Color.Lerp(from, to, progress);
			else
				return _lerpFun.Invoke(from, to, progress);
		}
	}

	/// <summary>
	/// QuaternionTween
	/// </summary>
	public sealed class QuaternionTween : TweenNode<Quaternion>
	{
		public static QuaternionTween Allocate(float duration, Quaternion from, Quaternion to)
		{
			return new QuaternionTween(duration, from, to);
		}

		public QuaternionTween(float duration, Quaternion from, Quaternion to) : base(duration, from, to)
		{
		}
		protected override Quaternion UpdateResultValue(Quaternion from, Quaternion to, float progress)
		{
			if (_lerpFun == null)
				return Quaternion.Lerp(from, to, progress);
			else
				return _lerpFun.Invoke(from, to, progress);
		}
	}
}