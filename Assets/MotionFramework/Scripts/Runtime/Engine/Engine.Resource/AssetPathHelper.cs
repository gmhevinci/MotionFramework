//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.IO;
using MotionFramework.IO;

namespace MotionFramework.Resource
{
	internal static class AssetPathHelper
	{
		private static string _cachedLowerLocationRoot;

		/// <summary>
		/// 获取规范化的路径
		/// </summary>
		public static string GetRegularPath(string path)
		{
			return path.Replace('\\', '/').Replace("\\", "/"); //替换为Linux路径格式
		}

		/// <summary>
		/// 获取文件所在的目录路径（Linux格式）
		/// </summary>
		public static string GetDirectory(string filePath)
		{
			string directory = Path.GetDirectoryName(filePath);
			return GetRegularPath(directory);
		}

		/// <summary>
		/// 获取基于流文件夹的加载路径
		/// </summary>
		public static string MakeStreamingLoadPath(string path)
		{
			return StringFormat.Format("{0}/{1}", UnityEngine.Application.streamingAssetsPath, path);
		}

		/// <summary>
		/// 获取基于沙盒文件夹的加载路径
		/// </summary>
		public static string MakePersistentLoadPath(string path)
		{
#if UNITY_EDITOR
			// 注意：为了方便调试查看，编辑器下把存储目录放到项目里
			string projectPath = GetDirectory(UnityEngine.Application.dataPath);
			return StringFormat.Format("{0}/Sandbox/{1}", projectPath, path);
#else
		return StringFormat.Format("{0}/Sandbox/{1}", UnityEngine.Application.persistentDataPath, path);
#endif
		}

		/// <summary>
		/// 获取网络资源加载路径
		/// </summary>
		public static string ConvertToWWWPath(string path)
		{
			// 注意：WWW加载方式，必须要在路径前面加file://
#if UNITY_EDITOR
			return StringFormat.Format("file:///{0}", path);
#elif UNITY_IPHONE
			return StringFormat.Format("file://{0}", path);
#elif UNITY_ANDROID
			return path;
#elif UNITY_STANDALONE
			return StringFormat.Format("file:///{0}", path);
#endif
		}

		/// <summary>
		/// 将资源定位地址转换为清单路径
		/// </summary>
		public static string ConvertLocationToManifestPath(string location, string variant)
		{
			// 注意：在编辑器模式下，加载路径是大小写敏感的
			if (_cachedLowerLocationRoot == null)
			{
				if (string.IsNullOrEmpty(AssetSystem.LocationRoot))
					throw new System.Exception($"{nameof(AssetSystem.LocationRoot)} is null or empty.");
				_cachedLowerLocationRoot = AssetSystem.LocationRoot.ToLower();
			}

			if (string.IsNullOrEmpty(variant))
				throw new System.Exception($"Variant is null or empty: {location}");

			return StringFormat.Format("{0}/{1}.{2}", _cachedLowerLocationRoot, location.ToLower(), variant.ToLower());
		}

		/// <summary>
		/// 获取AssetDatabase的加载路径
		/// </summary>
		public static string FindDatabaseAssetPath(string location)
		{
#if UNITY_EDITOR
			// 如果定位地址的资源是一个文件夹（代表这个文件夹内的所有资源在一个AssetBundle资源包里）
			string path = $"{AssetSystem.LocationRoot}/{location}";
			if (UnityEditor.AssetDatabase.IsValidFolder(path))
				return path;

			string fileName = Path.GetFileName(path);
			string folderPath = GetDirectory(path);
			string assetPath = FindDatabaseAssetPath(folderPath, fileName);
			if (string.IsNullOrEmpty(assetPath))
				return path;
			return assetPath;
#else
			return string.Empty;
#endif
		}

		/// <summary>
		/// 获取AssetDatabase的加载路径
		/// </summary>
		public static string FindDatabaseAssetPath(string folderPath, string fileName)
		{
#if UNITY_EDITOR
			// AssetDatabase加载资源需要提供文件后缀格式，然而资源定位地址并没有文件格式信息。
			// 所以我们通过查找该文件所在文件夹内同名的首个文件来确定AssetDatabase的加载路径。
			// 注意：AssetDatabase.FindAssets() 返回文件内包括递归文件夹内所有资源的GUID
			string[] guids = UnityEditor.AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);	

				if (UnityEditor.AssetDatabase.IsValidFolder(assetPath))
					continue;

				string directory = GetDirectory(assetPath);
				if (directory != folderPath)
					continue;

				string assetName = Path.GetFileNameWithoutExtension(assetPath);
				if (assetName == fileName)
					return assetPath;
			}
#endif
			// 没有找到同名的资源文件
			return string.Empty;
		}
	}
}