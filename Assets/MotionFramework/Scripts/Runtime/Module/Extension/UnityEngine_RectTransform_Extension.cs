//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace UnityEngine
{
	public static class UnityEngine_RectTransform_Extension
	{
		public static void SetSizeDeltaWidth(this RectTransform thisObj, float width)
		{
			Vector2 size = thisObj.sizeDelta;
			size.x = width;
			thisObj.sizeDelta = size;
		}
		public static void SetSizeDeltaHeight(this RectTransform thisObj, float height)
		{
			Vector2 size = thisObj.sizeDelta;
			size.y = height;
			thisObj.sizeDelta = size;
		}

		public static void SetAnchoredPositionX(this RectTransform thisObj, float pos)
		{
			Vector3 temp = thisObj.anchoredPosition;
			temp.x = pos;
			thisObj.anchoredPosition = temp;
		}
		public static void SetAnchoredPositionY(this RectTransform thisObj, float pos)
		{
			Vector3 temp = thisObj.anchoredPosition;
			temp.y = pos;
			thisObj.anchoredPosition = temp;
		}
		public static void SetAnchoredPositionZ(this RectTransform thisObj, float pos)
		{
			Vector3 temp = thisObj.anchoredPosition;
			temp.z = pos;
			thisObj.anchoredPosition = temp;
		}
	}
}