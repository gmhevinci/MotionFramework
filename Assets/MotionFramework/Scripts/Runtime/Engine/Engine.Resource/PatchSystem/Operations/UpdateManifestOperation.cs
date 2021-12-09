//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Network;

namespace MotionFramework.Resource
{
	/// <summary>
	/// 更新清单操作
	/// </summary>
	public abstract class UpdateManifestOperation : AsyncOperationBase
	{
	}

	/// <summary>
	/// 编辑器下模拟运行的更新清单操作
	/// </summary>
	internal class EditorModeUpdateManifestOperation : UpdateManifestOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeeded;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式的更新清单操作
	/// </summary>
	internal class OfflinePlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeeded;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 网络模式的更新清单操作
	/// </summary>
	internal class HostPlayModeUpdateManifestOperation : UpdateManifestOperation
	{
		private enum ESteps
		{
			Idle,
			LoadWebManifestHash,
			CheckWebManifestHash,
			LoadWebManifest,
			CheckWebManifest,
			Done,
		}

		private static int RequestCount = 0;

		private HostPlayModeImpl _impl;
		private int _updateResourceVersion;
		private int _timeout;
		private ESteps _steps = ESteps.Idle;
		private WebGetRequest _downloaderHash;
		private WebGetRequest _downloaderManifest;

		public HostPlayModeUpdateManifestOperation(HostPlayModeImpl impl, int updateResourceVersion, int timeout)
		{
			_impl = impl;
			_updateResourceVersion = updateResourceVersion;
			_timeout = timeout;
		}
		internal override void Start()
		{
			RequestCount++;
			_steps = ESteps.LoadWebManifestHash;

			if (_impl.IgnoreResourceVersion && _updateResourceVersion > 0)
			{
				MotionLog.Warning($"Update resource version {_updateResourceVersion} is invalid when ignore resource version.");
			}
			else
			{
				MotionLog.Log($"Update patch manifest : update resource version is  {_updateResourceVersion}");
			}
		}
		internal override void Update()
		{
			if (_steps == ESteps.Idle)
				return;

			if (_steps == ESteps.LoadWebManifestHash)
			{
				string webURL = GetPatchManifestRequestURL(_updateResourceVersion, ResourceSettingData.Setting.PatchManifestHashFileName);
				MotionLog.Log($"Beginning to request patch manifest hash : {webURL}");
				_downloaderHash = new WebGetRequest(webURL);
				_downloaderHash.SendRequest(_timeout);
				_steps = ESteps.CheckWebManifestHash;
			}

			if(_steps == ESteps.CheckWebManifestHash)
			{
				if (_downloaderHash.IsDone() == false)
					return;

				// Check fatal
				if (_downloaderHash.HasError())
				{	
					Error = _downloaderHash.GetError();					
					Status = EOperationStatus.Failed;
					_downloaderHash.Dispose();
					_steps = ESteps.Done;
					return;
				}

				// 获取补丁清单文件的哈希值
				string webManifestHash = _downloaderHash.GetText();
				_downloaderHash.Dispose();

				// 如果补丁清单文件的哈希值相同
				string currentFileHash = PatchHelper.GetSandboxPatchManifestFileHash();
				if (currentFileHash == webManifestHash)
				{
					MotionLog.Log($"Patch manifest file hash is not change : {webManifestHash}");
					Status = EOperationStatus.Succeeded;
					_steps = ESteps.Done;
					return;
				}
				else
				{
					MotionLog.Log($"Patch manifest hash is change : {webManifestHash} -> {currentFileHash}");
					_steps = ESteps.LoadWebManifest;
				}
			}

			if (_steps == ESteps.LoadWebManifest)
			{
				string webURL = GetPatchManifestRequestURL(_updateResourceVersion, ResourceSettingData.Setting.PatchManifestFileName);
				MotionLog.Log($"Beginning to request patch manifest : {webURL}");
				_downloaderManifest = new WebGetRequest(webURL);
				_downloaderManifest.SendRequest(_timeout);
				_steps = ESteps.CheckWebManifest;
			}

			if(_steps == ESteps.CheckWebManifest)
			{
				if (_downloaderManifest.IsDone() == false)
					return;

				// Check fatal
				if (_downloaderManifest.HasError())
				{
					Error = _downloaderManifest.GetError();
					Status = EOperationStatus.Failed;;
					_downloaderManifest.Dispose();
					_steps = ESteps.Done;
					return;
				}

				// 解析补丁清单			
				_impl.ParseAndSaveRemotePatchManifest(_downloaderManifest.GetText());
				_downloaderManifest.Dispose();
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeeded;
			}
		}

		private string GetPatchManifestRequestURL(int updateResourceVersion, string fileName)
		{
			string url;

			// 轮流返回请求地址
			if(RequestCount % 2 == 0)
				url = _impl.GetPatchDownloadFallbackURL(updateResourceVersion, fileName);
			else
				url = _impl.GetPatchDownloadURL(updateResourceVersion, fileName);

			// 注意：在URL末尾添加时间戳
			if (_impl.IgnoreResourceVersion)
				url = $"{url}?{System.DateTime.UtcNow.Ticks}";

			return url;
		}
	}
}