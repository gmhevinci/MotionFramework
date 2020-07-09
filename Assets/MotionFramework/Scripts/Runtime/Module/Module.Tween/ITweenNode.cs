//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Tween
{
	public interface ITweenNode
	{
		bool IsDone { get; }

		void OnUpdate();
		void OnDispose();
	}
}