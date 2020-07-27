//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Resource
{
	public class AssetBundleInfo
	{
		/// <summary>
		/// 清单路径
		/// </summary>
		public string ManifestPath { private set; get; }

		/// <summary>
		/// 本地存储的路径
		/// </summary>
		public string LocalPath { private set; get; }

		/// <summary>
		/// 远端下载的路径
		/// </summary>
		public string RemoteURL { private set; get; }

		/// <summary>
		/// 资源版本
		/// </summary>
		public int Version { private set; get; }

		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncrypted { private set; get; }

		public AssetBundleInfo(string manifestPath, string localPath, string remoteURL, int version, bool isEncrypted)
		{
			ManifestPath = manifestPath;
			LocalPath = localPath;
			RemoteURL = remoteURL;
			Version = version;
			IsEncrypted = isEncrypted;
		}
		public AssetBundleInfo(string manifestPath, string localPath)
		{
			ManifestPath = manifestPath;
			LocalPath = localPath;
			RemoteURL = string.Empty;
			Version = 0;
			IsEncrypted = false;
		}
	}
}