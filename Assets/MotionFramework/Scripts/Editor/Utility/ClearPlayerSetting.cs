//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public class ClearPlayerSetting
	{
		[MenuItem("MotionTools/Clear PlayerSetting")]
		static void ClearPlayerSettingFun()
		{
			PlayerPrefs.DeleteAll();
		}
	}
}