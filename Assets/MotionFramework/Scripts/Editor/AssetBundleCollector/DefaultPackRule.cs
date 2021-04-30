//--------------------------------------------------
// Motion Framework
// Copyright©2020-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 以文件路径作为AssetBundle标签名
	/// </summary>
	public class PackExplicit : IPackRule
	{
		string IPackRule.GetAssetBundleLabel(string assetPath)
		{
			return assetPath.RemoveExtension(); //"Assets/Config/test.txt" --> "Assets/Config/test"
		}
	}

	/// <summary>
	/// 以父文件夹路径作为AssetBundle标签名
	/// 注意：该文件夹下所有资源被打到一个AssetBundle文件里
	/// </summary>
	public class PackDirectory : IPackRule
	{
		string IPackRule.GetAssetBundleLabel(string assetPath)
		{
			return Path.GetDirectoryName(assetPath); //"Assets/Config/test.txt" --> "Assets/Config"
		}
	}
}