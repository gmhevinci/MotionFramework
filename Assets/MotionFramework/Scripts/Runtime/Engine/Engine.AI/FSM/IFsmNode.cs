//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.AI
{
	public interface IFsmNode
	{
		/// <summary>
		/// 节点名称
		/// </summary>
		string Name { get; }

		void OnEnter();
		void OnUpdate();
		void OnExit();
		void OnHandleMessage(object msg);
	}
}