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
	public class AnimPlayable
	{
		private readonly List<AnimState> _states = new List<AnimState>(10);
		private readonly List<AnimMixer> _mixers = new List<AnimMixer>(10);

		private PlayableGraph _graph;
		private AnimationPlayableOutput _output;
		private AnimationLayerMixerPlayable _mixerRoot;

		public void Create(Animator animator)
		{
			string name = animator.gameObject.name;
			_graph = PlayableGraph.Create(name);
			_graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);

			_mixerRoot = AnimationLayerMixerPlayable.Create(_graph);
			_output = AnimationPlayableOutput.Create(_graph, name, animator);
			_output.SetSourcePlayable(_mixerRoot);
		}
		public void Update(float deltaTime)
		{
			_graph.Evaluate(deltaTime);

			// 更新所有层级
			for (int i = 0; i < _mixers.Count; i++)
			{
				var mixer = _mixers[i];
				if(mixer.IsConnect)
					mixer.Update(deltaTime);
			}
		}
		public void Destroy()
		{
			_graph.Destroy();
		}

		/// <summary>
		/// Play the graph
		/// </summary>
		public void PlayGraph()
		{
			_graph.Play();
		}

		/// <summary>
		/// Stop the graph
		/// </summary>
		public void StopGraph()
		{
			_graph.Stop();
		}

		/// <summary>
		/// 检测动画是否正在播放
		/// </summary>
		/// <param name="name">动画名称</param>
		public bool IsPlaying(string name)
		{
			AnimState state = GetAnimState(name);
			if (state == null)
				return false;

			return state.IsConnect && state.IsPlaying;
		}

		/// <summary>
		/// 播放一个动画
		/// </summary>
		/// <param name="name">动画名称</param>
		/// <param name="fadeLength">融合时间</param>
		public void Play(string name, float fadeLength)
		{
			var animState = GetAnimState(name);
			if (animState == null)
			{
				MotionLog.Warning($"Not found animation {name}");
				return;
			}

			int layer = animState.Layer;
			var animMixer = GetAnimMixer(layer);
			if (animMixer == null)
				animMixer = CreateAnimMixer(layer);

			if(animMixer.IsConnect == false)
				animMixer.Connect(_mixerRoot, animMixer.Layer);		

			animMixer.Play(animState, fadeLength);
		}

		/// <summary>
		/// 停止一个动画
		/// </summary>
		/// <param name="name">动画名称</param>
		public void Stop(string name)
		{
			var animState = GetAnimState(name);
			if (animState == null)
			{
				MotionLog.Warning($"Not found animation {name}");
				return;
			}

			if (animState.IsConnect == false)
				return;

			var animMixer = GetAnimMixer(animState.Layer);
			if (animMixer == null)
				throw new System.Exception("Should never get here.");

			animMixer.Stop(animState.Name);
		}

		/// <summary>
		/// 添加一个动画片段
		/// </summary>
		/// <param name="name">动画名称</param>
		/// <param name="clip">动画片段</param>
		/// <param name="layer">动画层级</param>
		public bool AddAnimation(string name, AnimationClip clip, int layer = 0)
		{
			if (string.IsNullOrEmpty(name))
				throw new System.ArgumentException("Name is null or empty.");
			if (clip == null)
				throw new System.ArgumentNullException();
			if (layer < 0)
				throw new System.Exception("Layer must be greater than zero.");

			if (IsContains(name))
			{
				MotionLog.Warning($"Animation already exists : {name}");
				return false;
			}

			AnimState stateNode = new AnimState(_graph, clip, name, layer);
			_states.Add(stateNode);
			return true;
		}

		/// <summary>
		/// 移除一个动画片段
		/// </summary>
		/// <param name="name">动画名称</param>
		public bool RemoveAnimation(string name)
		{
			if (IsContains(name) == false)
			{
				MotionLog.Warning($"Not found Animation : {name}");
				return false;
			}

			AnimState animState = GetAnimState(name);
			AnimMixer animMixer = GetAnimMixer(animState.Layer);
			if(animMixer != null)
				animMixer.RemoveState(animState.Name);

			animState.Destroy();
			_states.Remove(animState);
			return true;
		}

		/// <summary>
		/// 获取一个动画状态
		/// </summary>
		/// <param name="name">动画名称</param>
		public AnimState GetAnimState(string name)
		{
			for (int i = 0; i < _states.Count; i++)
			{
				if (_states[i].Name == name)
					return _states[i];
			}
			return null;
		}

		/// <summary>
		/// 是否包含一个动画状态
		/// </summary>
		/// <param name="name">动画名称</param>
		public bool IsContains(string name)
		{
			for (int i = 0; i < _states.Count; i++)
			{
				if (_states[i].Name == name)
					return true;
			}
			return false;
		}

		private AnimMixer GetAnimMixer(int layer)
		{
			for (int i = 0; i < _mixers.Count; i++)
			{
				if (_mixers[i].Layer == layer)
					return _mixers[i];
			}
			return null;
		}
		private AnimMixer CreateAnimMixer(int layer)
		{
			// Increase input count
			int inputCount = _mixerRoot.GetInputCount();
			if(layer == 0 && inputCount == 0)
			{
				_mixerRoot.SetInputCount(1);
			}
			else
			{
				if (layer > inputCount - 1)
				{
					_mixerRoot.SetInputCount(layer + 1);
				}
			}

			var animMixer = new AnimMixer(_graph, layer);
			_mixers.Add(animMixer);
			return animMixer;
		}
	}
}