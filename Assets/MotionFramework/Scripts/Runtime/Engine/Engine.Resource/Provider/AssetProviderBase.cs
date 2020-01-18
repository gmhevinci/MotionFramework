//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Resource
{
	internal abstract class AssetProviderBase : IAssetProvider
	{
		protected AssetLoaderBase _owner { private set; get; }

		public string AssetName { private set; get; }
		public System.Type AssetType { private set; get; }
		public System.Object AssetObject { protected set; get; }
		public EAssetStates States { protected set; get; }
		public AssetOperationHandle Handle { private set; get; }
		public System.Action<AssetOperationHandle> Callback { set; get; }
		public bool IsDone
		{
			get
			{
				return States == EAssetStates.Success || States == EAssetStates.Fail;
			}
		}
		public bool IsValid
		{
			get
			{
				return _owner.IsDestroy == false;
			}
		}
		public virtual float Progress
		{
			get
			{
				return 0;
			}
		}


		public AssetProviderBase(AssetLoaderBase owner, string assetName, System.Type assetType)
		{
			_owner = owner;
			AssetName = assetName;
			AssetType = assetType;
			States = EAssetStates.None;
			Handle = new AssetOperationHandle(this);
		}

		/// <summary>
		/// 轮询更新
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// 异步操作任务
		/// </summary>
		System.Threading.Tasks.Task<object> IAssetProvider.Task
		{
			get
			{
				var handle = WaitHandle;
				return System.Threading.Tasks.Task.Factory.StartNew(o =>
				{
					handle.WaitOne();
					return AssetObject;
				}, this);
			}
		}

		// 异步操作相关
		private System.Threading.EventWaitHandle _waitHandle;
		private System.Threading.WaitHandle WaitHandle
		{
			get
			{
				if (_waitHandle == null)
					_waitHandle = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);
				_waitHandle.Reset();
				return _waitHandle;
			}
		}
		protected void InvokeCompletion()
		{
			Callback?.Invoke(Handle);
			_waitHandle?.Set();
		}
	}
}