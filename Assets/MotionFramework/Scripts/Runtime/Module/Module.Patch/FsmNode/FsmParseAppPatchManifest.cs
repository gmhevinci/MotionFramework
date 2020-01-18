//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.AI;
using MotionFramework.Resource;
using MotionFramework.Network;

namespace MotionFramework.Patch
{
	internal class FsmParseAppPatchManifest : IFsmNode
	{
		private readonly PatchManagerImpl _patcher;
		public string Name { private set; get; }

		public FsmParseAppPatchManifest(PatchManagerImpl patcher)
		{
			_patcher = patcher;
			Name = EPatchStates.ParseAppPatchManifest.ToString();
		}
		void IFsmNode.OnEnter()
		{
			PatchEventDispatcher.SendPatchStatesChangeMsg(EPatchStates.ParseAppPatchManifest);
			MotionEngine.StartCoroutine(DownLoad());
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

		private IEnumerator DownLoad()
		{
			// 解析APP里的补丁清单
			string filePath = AssetPathHelper.MakeStreamingLoadPath(PatchDefine.PatchManifestFileName);
			string url = AssetPathHelper.ConvertToWWWPath(filePath);
			WebDataRequest downloader = new WebDataRequest(url);
			yield return downloader.DownLoad();

			if (downloader.States == EWebRequestStates.Success)
			{
				PatchHelper.Log(ELogLevel.Log, "Parse app patch manifest.");
				_patcher.ParseAppPatchManifest(downloader.GetText());
				downloader.Dispose();
				_patcher.SwitchNext();
			}
			else
			{
				throw new System.Exception($"Fatal error : Failed download file : {url}");
			}
		}
	}
}