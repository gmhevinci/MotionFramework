//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Resource
{
	/// <summary>
	/// 资源提供者
	/// </summary>
	internal interface IAssetProvider
	{
		/// <summary>
		/// 资源对象的名称
		/// </summary>
		string AssetName { get; }

		/// <summary>
		/// 资源对象的类型
		/// </summary>
		System.Type AssetType { get; }

		/// <summary>
		/// 获取的资源对象
		/// </summary>
		System.Object AssetObject { get; }

		/// <summary>
		/// 当前的加载状态
		/// </summary>
		EAssetStates States { get; }

		/// <summary>
		/// 资源操作句柄
		/// </summary>
		AssetOperationHandle Handle { get; }

		/// <summary>
		/// 用户请求的回调
		/// </summary>
		System.Action<AssetOperationHandle> Callback { set; get; }

		/// <summary>
		/// 是否完毕（成功或失败）
		/// </summary>
		bool IsDone { get; }

		/// <summary>
		/// 是否有效（AssetFileLoader销毁会导致Provider无效）
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// 加载进度
		/// </summary>
		float Progress { get; }

		/// <summary>
		/// 轮询更新方法
		/// </summary>
		void Update();

		/// <summary>
		/// 异步操作任务
		/// </summary>
		System.Threading.Tasks.Task<object> Task { get; }
	}
}