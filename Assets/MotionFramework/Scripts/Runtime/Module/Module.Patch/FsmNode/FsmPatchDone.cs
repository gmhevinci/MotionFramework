//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.AI;

namespace MotionFramework.Patch
{
	internal class FsmPatchDone: IFsmNode
	{
		public string Name { private set; get; }

		public FsmPatchDone()
		{
			Name = EPatchStates.PatchDone.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStepsChangeMsg(EPatchStates.PatchDone);
		}
		void IFsmNode.OnUpdate()
		{
		}
		void IFsmNode.OnExit()
		{
		}
		void IFsmNode.OnHandleMessage(object msg)
		{
		}
	}
}