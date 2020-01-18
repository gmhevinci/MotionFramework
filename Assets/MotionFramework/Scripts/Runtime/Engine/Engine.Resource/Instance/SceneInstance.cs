//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;

namespace MotionFramework.Resource
{
	/// <summary>
	/// 扩展的场景实例对象
	/// </summary>
	public class SceneInstance
	{
		private AsyncOperation _asyncOp;

		public SceneInstance(AsyncOperation op)
		{
			_asyncOp = op;
		}

		/// <summary>
		/// UnityEngine场景对象
		/// </summary>
		public UnityEngine.SceneManagement.Scene Scene { internal set; get; }

		/// <summary>
		/// 激活场景
		/// 注意：如果传入的参数SceneInstanceParam.ActivateOnLoad=false，需要手动激活场景
		/// </summary>
		public void Activate()
		{
			_asyncOp.allowSceneActivation = true;
		}
	}

	/// <summary>
	/// 加载场景实体对象需要提供的参数类
	/// </summary>
	public class SceneInstanceParam : IAssetParam
	{
		/// <summary>
		/// 是否是附加场景
		/// </summary>
		public bool IsAdditive { set; get; }

		/// <summary>
		/// 加载完毕时是否主动激活
		/// </summary>
		public bool ActivateOnLoad { set; get; }
	}
}
