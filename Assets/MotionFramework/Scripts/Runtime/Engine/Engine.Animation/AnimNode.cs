//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace MotionFramework.Experimental.Animation
{
	public abstract class AnimNode
	{
		protected readonly PlayableGraph _graph;
		protected Playable _playable;
		protected Playable _parent;

		private float _fadeSpeed = 0f;
		private float _fadeWeight = 0f;
		private bool _isFading = false;

		private float _weight = 0f;
		private bool _isWeightDirty = true;

		/// <summary>
		/// 是否已经连接
		/// </summary>
		public bool IsConnect { get; private set; } = false;

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _playable.IsDone();
			}
		}

		/// <summary>
		/// 是否有效
		/// </summary>
		public bool IsValid
		{
			get
			{
				return _playable.IsValid();
			}
		}

		/// <summary>
		/// 时间轴
		/// </summary>
		public float Time
		{
			set
			{
				_playable.SetTime(value);
			}
			get
			{
				return (float)_playable.GetTime();
			}
		}

		/// <summary>
		/// 播放速度
		/// </summary>
		public float Speed
		{
			set
			{
				_playable.SetSpeed(value);
			}
			get
			{
				return (float)_playable.GetSpeed();
			}
		}

		/// <summary>
		/// 权重值
		/// </summary>
		public float Weight
		{
			set
			{
				if (_weight != value)
				{
					_weight = value;
					_isWeightDirty = true;
				}
			}
			get
			{
				return _weight;
			}
		}

		/// <summary>
		/// 输入端口
		/// </summary>
		public int InputPort { private set; get; }

		/// <summary>
		/// 节点状态
		/// </summary>
		public EAnimStates States
		{
			get
			{
				if (_playable.GetPlayState() == PlayState.Playing)
					return EAnimStates.Playing;
				else if (_playable.GetPlayState() == PlayState.Paused)
					return EAnimStates.Paused;
				else
					throw new System.NotImplementedException($"{_playable.GetPlayState()}");
			}
		}


		public AnimNode(PlayableGraph graph)
		{
			_graph = graph;
		}
		public virtual void Update(float deltaTime)
		{
			if (_isFading)
			{
				Weight = Mathf.MoveTowards(Weight, _fadeWeight, _fadeSpeed * deltaTime);
				if (Mathf.Approximately(Weight, _fadeWeight))
				{
					_isFading = false;
				}
			}

			if (_isWeightDirty)
			{
				_isWeightDirty = false;
				_parent.SetInputWeight(InputPort, Weight);
			}
		}
		public virtual void Destroy()
		{
			if (IsValid)
			{
				_graph.DestroySubgraph(_playable);
			}
		}
		public virtual void PlayNode()
		{
			_playable.SetDone(false);
			_playable.Play();
		}
		public virtual void PauseNode()
		{
			_playable.Pause();
		}
		public virtual void ResetNode()
		{
			_fadeSpeed = 0;
			_fadeWeight = 0;
			_isFading = false;

			Time = 0;
			Speed = 1;
			Weight = 0;

			// 注意：需要立刻重置权重值
			_parent.SetInputWeight(InputPort, Weight);
		}

		public void SetDone()
		{
			_playable.SetDone(true);
		}
		public void Connect(Playable parent, int inputPort)
		{
			if (IsConnect)
				throw new System.Exception("AnimNode is connected.");

			IsConnect = true;
			_parent = parent;
			InputPort = inputPort;

			// 注意：连接之前先重置
			ResetNode();
			_graph.Connect(_playable, 0, parent, inputPort);
		}
		public void Disconnect()
		{
			if (IsConnect == false)
				throw new System.Exception("AnimNode is disconnected.");

			IsConnect = false;
			_graph.Disconnect(_parent, InputPort);
		}
		public void StartFade(float fadeWeight, float fadeDuration)
		{
			if (fadeDuration <= 0)
				throw new System.ArgumentException("fade duration is invalid.");

			if (Mathf.Approximately(Weight, fadeWeight))
				return;

			//注意：保持统一的渐变速度
			_fadeSpeed = 1f / fadeDuration;
			_fadeWeight = fadeWeight;
			_isFading = true;
		}
	}
}