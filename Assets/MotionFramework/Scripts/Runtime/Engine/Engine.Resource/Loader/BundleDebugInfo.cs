//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Reference;

namespace MotionFramework.Resource
{
	internal class BundleDebugInfo : IReference
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { set; get; }

		/// <summary>
		/// 资源版本
		/// </summary>
		public int Version { set; get; }

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { set; get; }

		/// <summary>
		/// 加载状态
		/// </summary>
		public ELoaderStates States { set; get; }

		void IReference.OnRelease()
		{
			BundleName = null;
			Version = 0;
			RefCount = 0;
			States = ELoaderStates.None;
		}
	}
}