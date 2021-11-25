//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Resource
{
	public class UpdateManifestResult
	{
		public enum EStates
		{
			None,
			Failed,
			Succeed,
		}

		/// <summary>
		/// 当前状态
		/// </summary>
		public EStates States;

		/// <summary>
		/// 错误信息
		/// </summary>
		public string Error;

		// 请求次数
		internal int RequestCount = 0;

		internal void Reset()
		{
			States = EStates.None;
			Error = string.Empty;
		}
		internal static UpdateManifestResult CreateSucceedResult()
		{
			UpdateManifestResult result = new UpdateManifestResult();
			result.States = EStates.Succeed;
			return result;
		}
	}
}