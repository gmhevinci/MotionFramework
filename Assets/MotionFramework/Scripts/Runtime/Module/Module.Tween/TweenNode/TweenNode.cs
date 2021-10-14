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
		/// <summary>
		/// 补间委托方法
		/// </summary>
		/// <param name="t">运行时长</param>
		/// <param name="b">起始数值</param>
		/// <param name="c">变化差值</param>
		/// <param name="d">总时长</param>
		public delegate float TweenEaseDelegate(float t, float b, float c, float d);

		/// <summary>
		/// 插值委托方法
		/// </summary>
		/// <param name="from">起始数值</param>
		/// <param name="to">目标数值</param>
		/// <param name="progress">进度值</param>
		public delegate T TweenLerpDelegate(T from, T to, float progress);
		

		private readonly float _duration;
		private readonly T _from;
		private readonly T _to;

		private ETweenLoop _tweenLoop = ETweenLoop.None;
		private int _loopCount = -1;
		private int _loopCounter = 0;
		private float _timeReverse = 1f;
		private float _running = 0;
		private System.Action<T> _onUpdate = null;
		private System.Action _onDispose = null;
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
			_onDispose?.Invoke();
		}
		void ITweenNode.OnUpdate(float deltaTime)
		{
			_running += (deltaTime * _timeReverse);
			if (_duration > 0 && _running > 0 && _running < _duration)
			{
				float progress = _easeFun.Invoke(_running, 0, 1, _duration);
				Result = UpdateResultValue(_from, _to, progress);
				_onUpdate?.Invoke(Result);
			}
			else
			{
				if (_tweenLoop == ETweenLoop.None)
				{
					IsDone = true;
					Result = _to;
					_onUpdate?.Invoke(Result);
				}
				else if (_tweenLoop == ETweenLoop.Restart)
				{
					_running = 0;
					Result = _to;
					_onUpdate?.Invoke(Result);

					_loopCounter++;
					if (_loopCount > 0 && _loopCounter >= _loopCount)
						IsDone = true;
				}
				else if (_tweenLoop == ETweenLoop.PingPong)
				{
					_timeReverse *= -1;
					if(_timeReverse > 0)
					{
						_running = 0;
						Result = _from;
						_onUpdate?.Invoke(Result);

						// 注意：完整PingPong算一次
						_loopCounter++;
						if (_loopCount > 0 && _loopCounter >= _loopCount)
							IsDone = true;
					}
					else
					{
						_running = _duration;
						Result = _to;
						_onUpdate?.Invoke(Result);
					}
				}
				else
				{
					throw new System.NotImplementedException();
				}
			}
		}
		void ITweenNode.Kill()
		{
			IsDone = true;
		}

		public TweenNode<T> SetLoop(ETweenLoop tweenLoop, int loopCount = -1)
		{
			_tweenLoop = tweenLoop;
			_loopCount = loopCount;
			return this;
		}
		public TweenNode<T> SetEase(AnimationCurve easeCurve)
		{
			if (easeCurve == null)
			{
				MotionLog.Error("AnimationCurve is null. Tween ease function use default.");
				_easeFun = TweenEase.Linear.Default;
				return this;
			}

			// 获取动画总时长
			float length = 0f;
			for (int i = 0; i < easeCurve.keys.Length; i++)
			{
				var key = easeCurve.keys[i];
				if (key.time > length)
					length = key.time;
			}

			_easeFun = delegate (float t, float b, float c, float d)
			{
				float time = length * (t / d);
				return easeCurve.Evaluate(time) * c + b;
			};

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
		public TweenNode<T> SetUpdate(System.Action<T> onUpdate)
		{
			_onUpdate = onUpdate;
			return this;
		}
		public TweenNode<T> SetDispose(System.Action onDispose)
		{
			_onDispose = onDispose;
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
				return Mathf.LerpUnclamped(from, to, progress);
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
				return Vector2.LerpUnclamped(from, to, progress);
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
				return Vector3.LerpUnclamped(from, to, progress);
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
				return Vector4.LerpUnclamped(from, to, progress);
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
				return Color.LerpUnclamped(from, to, progress);
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
				return Quaternion.LerpUnclamped(from, to, progress);
			else
				return _lerpFun.Invoke(from, to, progress);
		}
	}
}