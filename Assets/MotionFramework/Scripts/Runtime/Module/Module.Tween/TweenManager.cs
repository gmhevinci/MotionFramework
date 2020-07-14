//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;
using MotionFramework.Console;

namespace MotionFramework.Tween
{
	/// <summary>
	/// 补间管理器
	/// </summary>
	public class TweenManager : ModuleSingleton<TweenManager>, IModule
	{
		private class TweenWrapper
		{
			public int GroupID { private set; get; }
			public long TweenUID { private set; get; }
			public ITweenNode TweenRoot { private set; get; }
			public UnityEngine.Object SafeObject { private set; get; }
			private readonly bool _safeMode = false;

			public TweenWrapper(int groupID, long tweenUID, ITweenNode tweenRoot, UnityEngine.Object safeObject)
			{
				GroupID = groupID;
				TweenUID = tweenUID;
				TweenRoot = tweenRoot;
				SafeObject = safeObject;
				_safeMode = safeObject != null;
			}
			public bool IsSafe()
			{
				if (_safeMode == false)
					return true;
				return SafeObject != null;
			}
		}

		private static int StaticTweenUID = 0;
		private readonly List<TweenWrapper> _wrappers = new List<TweenWrapper>(1000);
		private readonly List<TweenWrapper> _remover = new List<TweenWrapper>(1000);

		void IModule.OnCreate(object createParam)
		{
		}
		void IModule.OnUpdate()
		{
			_remover.Clear();

			// 更新所有补间动画
			for (int i = 0; i < _wrappers.Count; i++)
			{
				var wrapper = _wrappers[i];
				if (wrapper.IsSafe() == false)
				{
					wrapper.TweenRoot.Kill();
					_remover.Add(wrapper);
					continue;
				}

				if (wrapper.TweenRoot.IsDone)
					_remover.Add(wrapper);
				else
					wrapper.TweenRoot.OnUpdate();
			}

			// 移除完成的补间动画
			for (int i = 0; i < _remover.Count; i++)
			{
				var wrapper = _remover[i];
				_wrappers.Remove(wrapper);
				wrapper.TweenRoot.OnDispose();
			}
		}
		void IModule.OnGUI()
		{
			ConsoleGUI.Lable($"[{nameof(TweenManager)}] Tween total count : {_wrappers.Count}");
		}

		/// <summary>
		/// 播放一个补间动画
		/// </summary>
		/// <param name="tweenRoot">补间根节点</param>
		/// <param name="safeObject">安全游戏对象：如果安全游戏对象被销毁，补间动画会自动终止</param>
		/// <param name="groupID">补间组ID</param>
		/// <returns>补间动画唯一ID</returns>
		public long Play(ITweenNode tweenRoot, UnityEngine.Object safeObject = null, int groupID = 0)
		{
			if (tweenRoot == null)
			{
				MotionLog.Warning("Tween root is null.");
				return -1;
			}

			if (Contains(tweenRoot))
			{
				MotionLog.Warning("Tween root is running.");
				return -1;
			}

			long tweenUID = ++StaticTweenUID;
			TweenWrapper wrapper = new TweenWrapper(groupID, tweenUID, tweenRoot, safeObject);
			_wrappers.Add(wrapper);
			return wrapper.TweenUID;
		}

		/// <summary>
		/// 中途关闭一个补间动画
		/// </summary>
		/// <param name="tweenUID">补间动画唯一ID</param>
		public void Kill(long tweenUID)
		{
			TweenWrapper wrapper = GetTweenWrapper(tweenUID);
			if (wrapper != null)
				wrapper.TweenRoot.Kill();
		}

		/// <summary>
		/// 中途关闭一组补间动画
		/// </summary>
		/// <param name="groupID">补间组ID</param>
		public void Kill(int groupID)
		{
			for (int i = 0; i < _wrappers.Count; i++)
			{
				var wrapper = _wrappers[i];
				if (wrapper.GroupID != 0 && wrapper.GroupID == groupID)
					wrapper.TweenRoot.Kill();
			}
		}

		private bool Contains(ITweenNode tweenRoot)
		{
			for (int i = 0; i < _wrappers.Count; i++)
			{
				var wrapper = _wrappers[i];
				if (wrapper.TweenRoot == tweenRoot)
					return true;
			}
			return false;
		}
		private TweenWrapper GetTweenWrapper(long tweenUID)
		{
			for (int i = 0; i < _wrappers.Count; i++)
			{
				var wrapper = _wrappers[i];
				if (wrapper.TweenUID == tweenUID)
					return wrapper;
			}
			return null;
		}
	}
}