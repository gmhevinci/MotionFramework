//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using MotionFramework.Window;

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

	/// <summary>
	/// 异形屏支持
	/// </summary>
	/// <param name="landscape">是否是横屏</param>
	/// <param name="offset">偏移值</param>
	public void NotchSupport(bool landscape, float offset)
	{
		var rectTrans = this.UIDesktop.transform as RectTransform;
		if (landscape)
		{
			rectTrans.offsetMin = new Vector2(offset, 0);
			rectTrans.offsetMax = new Vector2(-offset, 0);
		}
		else
		{
			rectTrans.offsetMin = new Vector2(0, offset);
			rectTrans.offsetMax = new Vector2(0, -offset);
		}
	}

	protected override void OnAssetLoad(GameObject go)
	{
		UIDesktop = Go.transform.BFSearch("UIDesktop").gameObject;
		UICamera = Go.transform.BFSearch("UICamera").GetComponent<Camera>();
	}
}