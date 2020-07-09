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
		private readonly float _duration;
		private readonly T _from;
		private readonly T _to;

		private float _running = 0;
		private bool _ignoreTimeScale = false;
		private System.Action<T> _update = null;
		protected TweenEaseDelegate _easeFun;

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
				Result = UpdateEase(_from, _to, _running, _duration);
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
		public TweenNode<T> SetUpdate(System.Action<T> update)
		{
			_update = update;
			return this;
		}

		protected abstract T UpdateEase(T from, T to, float running, float duration);
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
		protected override float UpdateEase(float from, float to, float running, float duration)
		{
			return _easeFun.Invoke(running, from, (to - from), duration);
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
		protected override Vector2 UpdateEase(Vector2 from, Vector2 to, float running, float duration)
		{
			float x = _easeFun.Invoke(running, from.x, (to.x - from.x), duration);
			float y = _easeFun.Invoke(running, from.y, (to.y - from.y), duration);
			return new Vector2(x, y);
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
		protected override Vector3 UpdateEase(Vector3 from, Vector3 to, float running, float duration)
		{
			float x = _easeFun.Invoke(running, from.x, (to.x - from.x), duration);
			float y = _easeFun.Invoke(running, from.y, (to.y - from.y), duration);
			float z = _easeFun.Invoke(running, from.z, (to.z - from.z), duration);
			return new Vector3(x, y, z);
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
		protected override Vector4 UpdateEase(Vector4 from, Vector4 to, float running, float duration)
		{
			float x = _easeFun.Invoke(running, from.x, (to.x - from.x), duration);
			float y = _easeFun.Invoke(running, from.y, (to.y - from.y), duration);
			float z = _easeFun.Invoke(running, from.z, (to.z - from.z), duration);
			float w = _easeFun.Invoke(running, from.w, (to.w - from.w), duration);
			return new Vector4(x, y, z, w);
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
		protected override Color UpdateEase(Color from, Color to, float running, float duration)
		{
			float r = _easeFun.Invoke(running, from.r, (to.r - from.r), duration);
			float g = _easeFun.Invoke(running, from.g, (to.g - from.g), duration);
			float b = _easeFun.Invoke(running, from.b, (to.b - from.b), duration);
			float a = _easeFun.Invoke(running, from.a, (to.a - from.a), duration);
			return new Color(r, g, b, a);
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
		protected override Quaternion UpdateEase(Quaternion from, Quaternion to, float running, float duration)
		{
			float x = _easeFun.Invoke(running, from.x, (to.x - from.x), duration);
			float y = _easeFun.Invoke(running, from.y, (to.y - from.y), duration);
			float z = _easeFun.Invoke(running, from.z, (to.z - from.z), duration);
			float w = _easeFun.Invoke(running, from.w, (to.w - from.w), duration);
			return new Quaternion(x, y, z, w);
		}
	}
}