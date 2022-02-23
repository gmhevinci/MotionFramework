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
	public sealed class AnimMixer : AnimNode
	{
		private const float HIDE_DURATION = 0.25f;
		private readonly List<AnimState> _states = new List<AnimState>(10);
		private AnimationMixerPlayable _mixer;
		private bool _isQuiting = false;

		/// <summary>
		/// 动画层级
		/// </summary>
		public int Layer { private set; get; }


		public AnimMixer(PlayableGraph graph, int layer) : base(graph)
		{
			Layer = layer;

			_mixer = AnimationMixerPlayable.Create(graph);
			SetSourcePlayable(_mixer);
		}
		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			for (int i = 0; i < _states.Count; i++)
			{
				var state = _states[i];
				if (state != null)
					state.Update(deltaTime);
			}

			bool isAllDone = true;
			for (int i = 0; i < _states.Count; i++)
			{
				var state = _states[i];
				if (state != null)
				{
					if (state.IsDone == false)
						isAllDone = false;
				}
			}

			// 当子节点都已经完成的时候断开连接
			if (isAllDone && _isQuiting == false)
			{
				_isQuiting = true;
				StartWeightFade(0, HIDE_DURATION);
			}
			if (_isQuiting)
			{
				if (Mathf.Approximately(Weight, 0f))
					DisconnectMixer();
			}
		}

		/// <summary>
		/// 播放指定动画
		/// </summary>
		public void Play(AnimState animState, float fadeDuration)
		{
			// 重新激活混合器
			_isQuiting = false;
			StartWeightFade(1f, 0);

			if (IsContains(animState) == false)
			{
				// 优先插入到一个空位
				int index = _states.FindIndex(s => s == null);
				if (index == -1)
				{
					// Increase input count
					int inputCount = _mixer.GetInputCount();
					_mixer.SetInputCount(inputCount + 1);

					animState.Connect(_mixer, inputCount);
					_states.Add(animState);
				}
				else
				{
					animState.Connect(_mixer, index);
					_states[index] = animState;
				}
			}

			for (int i = 0; i < _states.Count; i++)
			{
				var state = _states[i];
				if (state == null)
					continue;

				if (state == animState)
				{
					state.StartWeightFade(1f, fadeDuration);
					state.PlayNode();
				}
				else
				{
					state.StartWeightFade(0f, fadeDuration);
					state.PauseNode();
				}
			}
		}

		/// <summary>
		/// 停止指定动画，恢复为初始状态
		/// </summary>
		public void Stop(string name)
		{
			AnimState state = FindState(name);
			if (state == null)
				return;

			state.PauseNode();
			state.ResetNode();
		}

		/// <summary>
		/// 暂停所有动画
		/// </summary>
		public void PauseAll()
		{
			for (int i = 0; i < _states.Count; i++)
			{
				var state = _states[i];
				if (state == null)
					continue;
				state.PauseNode();
			}
		}

		/// <summary>
		/// 是否包含该动画
		/// </summary>
		public bool IsContains(AnimNode node)
		{
			foreach (var state in _states)
			{
				if (state == node)
					return true;
			}
			return false;
		}

		/// <summary>
		/// 移除一个动画
		/// </summary>
		public void RemoveState(string name)
		{
			var state = FindState(name);
			if (state == null)
				return;

			_states[state.InputPort] = null;
			state.Destroy();
		}

		/// <summary>
		/// 获取指定的动画
		/// </summary>
		/// <returns>如果没有返回NULL</returns>
		private AnimState FindState(string name)
		{
			foreach (var state in _states)
			{
				if (state != null && state.Name == name)
					return state;
			}

			MotionLog.Warning($"Animation state doesn't exist : {name}");
			return null;
		}

		private void DisconnectMixer()
		{
			for (int i = 0; i < _states.Count; i++)
			{
				var state = _states[i];
				if (state == null)
					continue;

				state.Disconnect();
				_states[i] = null;
			}

			Disconnect();
		}
	}
}