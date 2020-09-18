//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using MotionFramework.Window;
using MotionFramework;
using UnityEngine.UI;

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

	public CanvasRoot()
	{
	}
	protected override void OnAssetLoad(GameObject go)
	{
		var desktopTrans = Go.transform.BFSearch("UIDesktop");
		if (desktopTrans != null)
			UIDesktop = desktopTrans.gameObject;
		else
			MotionLog.Error("Not found UIDesktop gameObject in UIRoot");

		var cameraTrans = Go.transform.BFSearch("UICamera");
		if (cameraTrans != null)
			UICamera = cameraTrans.GetComponent<Camera>();
		else
			MotionLog.Error("Not found UICamera gameObject in UIRoot");
	}

	/// <summary>
	/// 设置屏幕安全区域（异形屏支持）
	/// </summary>
	/// <param name="safeRect">安全区域</param>
	public void ApplySafeRect(Rect safeRect)
	{
		// 注意：安全区坐标系的原点为左下角
		var rectTrans = this.UIDesktop.transform as RectTransform;
		CanvasScaler scaler = Go.GetComponent<CanvasScaler>();

		// Convert safe area rectangle from absolute pixels to UGUI coordinates
		float rateX = scaler.referenceResolution.x / Screen.width;
		float rateY = scaler.referenceResolution.y / Screen.height;
		float posX = (int)(safeRect.position.x * rateX);
		float posY = (int)(safeRect.position.y * rateY);
		float width = (int)(safeRect.size.x * rateX);
		float height = (int)(safeRect.size.y * rateY);

		float offsetMaxX = scaler.referenceResolution.x - width - posX;
		float offsetMaxY = scaler.referenceResolution.y - height - posY;
		rectTrans.offsetMin = new Vector2(posX, posY); //锚框状态下的屏幕左下角偏移向量
		rectTrans.offsetMax = new Vector2(-offsetMaxX, -offsetMaxY); //锚框状态下的屏幕右上角偏移向量
	}

	/// <summary>
	/// 编辑器下模拟IPhoneX异形屏
	/// </summary>
	public void SimulateIPhoneXNotchScreenOnEditor()
	{
#if UNITY_EDITOR
		Rect rect;
		if (Screen.height > Screen.width)
		{
			// 竖屏Portrait
			float deviceWidth = 1125;
			float deviceHeight = 2436;
			rect = new Rect(0f / deviceWidth, 102f / deviceHeight, 1125f / deviceWidth, 2202f / deviceHeight);
		}
		else
		{
			// 横屏Landscape
			float deviceWidth = 2436;
			float deviceHeight = 1125;
			rect = new Rect(132f / deviceWidth, 63f / deviceHeight, 2172f / deviceWidth, 1062f / deviceHeight);
		}

		Rect safeArea = new Rect(Screen.width * rect.x, Screen.height * rect.y, Screen.width * rect.width, Screen.height * rect.height);
		ApplySafeRect(safeArea);
#endif
	}
}