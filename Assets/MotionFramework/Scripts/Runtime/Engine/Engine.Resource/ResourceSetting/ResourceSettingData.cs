//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Resource
{
	public static class ResourceSettingData
	{
		private static ResourceSetting _setting = null;
		public static ResourceSetting Setting
		{
			get
			{
				if (_setting == null)
					LoadSettingData();
				return _setting;
			}
		}

		/// <summary>
		/// 加载配置文件
		/// </summary>
		private static void LoadSettingData()
		{
			_setting = Resources.Load<ResourceSetting>("ResourceSetting");
			if (_setting == null)
			{
				Debug.Log("use default resource setting.");
				_setting = ScriptableObject.CreateInstance<ResourceSetting>();
			}

			// 注意：设置为常驻对象
			_setting.hideFlags = HideFlags.HideAndDontSave;
		}
	}
}