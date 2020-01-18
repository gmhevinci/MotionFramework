//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Config
{
	internal class ConfigDefine
	{
		/// <summary>
		/// 配表文件最大128MB
		/// </summary>
		public const int CfgStreamMaxLen = 1024 * 1024 * 128;

		/// <summary>
		/// 配表单行最大256K
		/// </summary>
		public const int TabStreamMaxLen = 1024 * 256;

		/// <summary>
		/// 配表文件标记
		/// </summary>
		public const short TabStreamHead = 0x2B2B;
	}
}