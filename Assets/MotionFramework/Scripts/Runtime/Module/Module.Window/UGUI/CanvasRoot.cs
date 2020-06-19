//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Window
{
    public class CanvasRoot : UIRoot
	{
		/// <summary>
		/// UI桌面
		/// </summary>
		public override GameObject UIDesktop { protected set; get; }

		/// <summary>
		/// UI相机
		/// </summary>
		public override Camera UICamera { protected set; get; }

		protected override void OnAssetLoad(GameObject go)
        {
            UIDesktop = Go.transform.BFSearch("UIDesktop").gameObject;
            UICamera = Go.transform.BFSearch("UICamera").GetComponent<Camera>();
        }
	}
}