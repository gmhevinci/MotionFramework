//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Patch
{
	/// <summary>
	/// 补丁文件校验等级
	/// </summary>
	public enum EVerifyLevel
	{
		/// <summary>
		/// 验证文件大小
		/// </summary>
		Size,

		/// <summary>
		/// 验证文件哈希值
		/// </summary>
		Hash,
	}
}