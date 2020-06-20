//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Editor
{
	public class UIPanelSetting : ScriptableObject
	{
		/// <summary>
		/// 精灵文件夹路径
		/// </summary>
		public string UISpriteDirectory = string.Empty;
		
		/// <summary>
		/// 图集文件夹路径
		/// </summary>
		public string UIAtlasDirectory = string.Empty;
	}
}