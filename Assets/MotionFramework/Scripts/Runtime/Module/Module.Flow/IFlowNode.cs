//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Flow
{
	public interface IFlowNode
	{
		bool IsDone { get; }

		void OnUpdate();
		void OnDispose();
	}
}