//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Resource
{
	internal abstract class AssetProviderBase : IAssetProvider
	{
		protected AssetLoaderBase Owner { private set; get; }

		public string AssetName { private set; get; }
		public System.Type AssetType { private set; get; }
		public UnityEngine.Object AssetObject { protected set; get; }
		public UnityEngine.Object[] AllAssets { protected set; get; }
		public IAssetInstance AssetInstance { protected set; get; }
		public EAssetStates States { protected set; get; }
		public int RefCount { private set; get; }
		public AssetOperationHandle Handle { private set; get; }
		public System.Action<AssetOperationHandle> Callback { set; get; }
		public bool IsDestroyed { private set; get; } = false;
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
				//注意：当AssetBundle被强制卸载后，所有AssetProvider失效
				return IsDestroyed == false && Owner.IsDestroyed == false;
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
			Owner = owner;
			AssetName = assetName;
			AssetType = assetType;
			States = EAssetStates.None;
			Handle = new AssetOperationHandle(this);
		}
		
		public abstract void Update();
		public virtual void Destory()
		{
			IsDestroyed = true;
		}
		public virtual void ForceSyncLoad()
		{
		}

		public void Reference()
		{
			RefCount++;
			Owner.Reference();
		}
		public void Release()
		{
			if (RefCount <= 0)
				throw new System.Exception("Cannot decrement reference count, AssetProvider reference is already zero.");

			RefCount--;
			Owner.Release();
		}
		public bool CanDestroy()
		{
			if (IsDone == false)
				return false;

			return RefCount <= 0;
		}

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
					return AssetObject as object;
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