//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MotionFramework.Patch;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	public class AssetBundleBuilder
	{
		/// <summary>
		/// AssetBundle压缩选项
		/// </summary>
		public enum ECompressOption
		{
			Uncompressed = 0,
			StandardCompressionLZMA,
			ChunkBasedCompressionLZ4,
		}

		/// <summary>
		/// 输出的根目录
		/// </summary>
		private readonly string _outputRoot;

		// 构建相关
		public BuildTarget BuildTarget { private set; get; } = BuildTarget.NoTarget;
		public int BuildVersion { set; get; } = -1;
		public string OutputPath { private set; get; } = string.Empty;

		// 构建选项
		public ECompressOption CompressOption = ECompressOption.Uncompressed;
		public bool IsForceRebuild = false;
		public bool IsAppendHash = false;
		public bool IsDisableWriteTypeTree = false;
		public bool IsIgnoreTypeTreeChanges = false;


		/// <summary>
		/// AssetBuilder
		/// </summary>
		/// <param name="buildTarget">构建平台</param>
		/// <param name="buildVersion">构建版本</param>
		public AssetBundleBuilder(BuildTarget buildTarget, int buildVersion)
		{
			_outputRoot = AssetBundleBuilderHelper.GetDefaultOutputRootPath();
			BuildTarget = buildTarget;
			BuildVersion = buildVersion;
			OutputPath = GetOutputPath();
		}

		/// <summary>
		/// 准备构建
		/// </summary>
		public void PreAssetBuild()
		{
			Debug.Log("------------------------------OnPreAssetBuild------------------------------");

			// 检测构建平台是否合法
			if (BuildTarget == BuildTarget.NoTarget)
				throw new Exception("[BuildPatch] 请选择目标平台");

			// 检测构建版本是否合法
			if (EditorTools.IsNumber(BuildVersion.ToString()) == false)
				throw new Exception($"[BuildPatch] 版本号格式非法：{BuildVersion}");
			if (BuildVersion < 0)
				throw new Exception("[BuildPatch] 请先设置版本号");

			// 检测输出目录是否为空
			if (string.IsNullOrEmpty(OutputPath))
				throw new Exception("[BuildPatch] 输出目录不能为空");

			// 检测补丁包是否已经存在
			string packageFolderPath = GetPackageFolderPath();
			if (Directory.Exists(packageFolderPath))
				throw new Exception($"[BuildPatch] 补丁包已经存在：{packageFolderPath}");

			// 如果是强制重建
			if (IsForceRebuild)
			{
				// 删除总目录
				string parentPath = $"{_outputRoot}/{BuildTarget}";
				if (Directory.Exists(parentPath))
				{
					Directory.Delete(parentPath, true);
					Log($"删除平台总目录：{parentPath}");
				}
			}

			// 如果输出目录不存在
			if (Directory.Exists(OutputPath) == false)
			{
				Directory.CreateDirectory(OutputPath);
				Log($"创建输出目录：{OutputPath}");
			}
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		public void PostAssetBuild()
		{
			Debug.Log("------------------------------OnPostAssetBuild------------------------------");

			// 准备工作
			List<AssetBundleBuild> buildInfoList = new List<AssetBundleBuild>();
			List<AssetInfo> buildMap = GetBuildMap();
			if (buildMap.Count == 0)
				throw new Exception("[BuildPatch] 构建列表不能为空");

			Log($"构建列表里总共有{buildMap.Count}个资源需要构建");
			for (int i = 0; i < buildMap.Count; i++)
			{
				AssetInfo assetInfo = buildMap[i];
				AssetBundleBuild buildInfo = new AssetBundleBuild();
				buildInfo.assetBundleName = assetInfo.AssetBundleLabel;
				buildInfo.assetBundleVariant = assetInfo.AssetBundleVariant;
				buildInfo.assetNames = new string[] { assetInfo.AssetPath };
				buildInfoList.Add(buildInfo);
			}

			// 开始构建
			Log($"开始构建......");
			BuildAssetBundleOptions opt = MakeBuildOptions();
			AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(OutputPath, buildInfoList.ToArray(), opt, BuildTarget);
			if (unityManifest == null)
				throw new Exception("[BuildPatch] 构建过程中发生错误！");

			// 视频单独打包
			PackVideo(buildMap);
			// 加密资源文件
			EncryptFiles(unityManifest);

			// 创建补丁文件
			CreatePatchManifestFile(unityManifest);
			// 创建说明文件
			CreateReadmeFile(unityManifest);

			// 复制更新文件到新的补丁文件夹
			CopyUpdateFiles();

			Log("构建完成");
		}

		/// <summary>
		/// 获取构建选项
		/// </summary>
		private BuildAssetBundleOptions MakeBuildOptions()
		{
			// For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
			// 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包

			BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
			opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.

			if (CompressOption == ECompressOption.Uncompressed)
				opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
			else if (CompressOption == ECompressOption.ChunkBasedCompressionLZ4)
				opt |= BuildAssetBundleOptions.ChunkBasedCompression;

			if (IsForceRebuild)
				opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
			if (IsAppendHash)
				opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName; //Append the hash to the assetBundle name
			if (IsDisableWriteTypeTree)
				opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
			if (IsIgnoreTypeTreeChanges)
				opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.

			return opt;
		}

		private void Log(string log)
		{
			Debug.Log($"[BuildPatch] {log}");
		}
		private string GetOutputPath()
		{
			return $"{_outputRoot}/{BuildTarget}/{PatchDefine.UnityManifestFileName}";
		}
		private string GetPackageFolderPath()
		{
			return $"{_outputRoot}/{BuildTarget}/{BuildVersion}";
		}

		#region 准备工作
		/// <summary>
		/// 准备工作
		/// </summary>
		private List<AssetInfo> GetBuildMap()
		{
			int progressBarCount = 0;
			Dictionary<string, AssetInfo> allAsset = new Dictionary<string, AssetInfo>();

			// 获取所有的收集路径
			List<string> collectPathList = AssetBundleCollectorSettingData.GetAllCollectPath();
			if (collectPathList.Count == 0)
				throw new Exception("[BuildPatch] 配置的打包路径列表为空");

			// 获取所有资源
			string[] guids = AssetDatabase.FindAssets(string.Empty, collectPathList.ToArray());
			foreach (string guid in guids)
			{
				string mainAssetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (AssetBundleCollectorSettingData.IsIgnoreAsset(mainAssetPath))
					continue;
				if (ValidateAsset(mainAssetPath) == false)
					continue;

				List<AssetInfo> depends = GetDependencies(mainAssetPath);
				for (int i = 0; i < depends.Count; i++)
				{
					AssetInfo assetInfo = depends[i];
					if (allAsset.ContainsKey(assetInfo.AssetPath))
					{
						AssetInfo cacheInfo = allAsset[assetInfo.AssetPath];
						cacheInfo.DependCount++;
					}
					else
					{
						allAsset.Add(assetInfo.AssetPath, assetInfo);
					}
				}

				// 进度条
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"依赖文件分析：{progressBarCount}/{guids.Length}", (float)progressBarCount / guids.Length);
			}
			EditorUtility.ClearProgressBar();
			progressBarCount = 0;

			// 移除零依赖的资源
			List<string> removeList = new List<string>();
			foreach (KeyValuePair<string, AssetInfo> pair in allAsset)
			{
				if (pair.Value.IsCollectAsset)
					continue;
				if (pair.Value.DependCount == 0)
					removeList.Add(pair.Value.AssetPath);
			}
			for (int i = 0; i < removeList.Count; i++)
			{
				allAsset.Remove(removeList[i]);
			}

			// 设置资源标签
			foreach (KeyValuePair<string, AssetInfo> pair in allAsset)
			{
				SetAssetBundleLabelAndVariant(pair.Value);

				// 进度条
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"设置资源标签：{progressBarCount}/{allAsset.Count}", (float)progressBarCount / allAsset.Count);
			}
			EditorUtility.ClearProgressBar();
			progressBarCount = 0;

			// 返回结果
			return allAsset.Values.ToList();
		}

		/// <summary>
		/// 获取指定资源依赖的资源列表
		/// 注意：返回列表里已经包括主资源自己
		/// </summary>
		private List<AssetInfo> GetDependencies(string assetPath)
		{
			List<AssetInfo> depends = new List<AssetInfo>();
			string[] dependArray = AssetDatabase.GetDependencies(assetPath, true);
			foreach (string dependPath in dependArray)
			{
				if (ValidateAsset(dependPath))
				{
					AssetInfo assetInfo = new AssetInfo(dependPath);
					depends.Add(assetInfo);
				}
			}
			return depends;
		}

		/// <summary>
		/// 检测资源是否有效
		/// </summary>
		private bool ValidateAsset(string assetPath)
		{
			if (!assetPath.StartsWith("Assets/"))
				return false;

			if (AssetDatabase.IsValidFolder(assetPath))
				return false;

			string ext = System.IO.Path.GetExtension(assetPath);
			if (ext == "" || ext == ".dll" || ext == ".cs" || ext == ".js" || ext == ".boo" || ext == ".meta")
				return false;

			return true;
		}

		/// <summary>
		/// 设置资源的标签和变种
		/// </summary>
		private void SetAssetBundleLabelAndVariant(AssetInfo assetInfo)
		{
			// 如果资源所在文件夹的名称包含后缀符号，则为变体资源
			string folderName = Path.GetDirectoryName(assetInfo.AssetPath); // "Assets/Texture.HD/background.jpg" --> "Assets/Texture.HD"
			if (Path.HasExtension(folderName))
			{
				string extension = Path.GetExtension(folderName);
				string label = AssetBundleCollectorSettingData.GetAssetBundleLabel(assetInfo.AssetPath);
				assetInfo.AssetBundleLabel = label.Replace(extension, string.Empty);
				assetInfo.AssetBundleVariant = extension.Substring(1);
			}
			else
			{
				assetInfo.AssetBundleLabel = AssetBundleCollectorSettingData.GetAssetBundleLabel(assetInfo.AssetPath);
				assetInfo.AssetBundleVariant = PatchDefine.AssetBundleDefaultVariant;
			}
		}
		#endregion

		#region 视频相关
		private void PackVideo(List<AssetInfo> buildMap)
		{
			// 注意：在Unity2018.4截止的版本里，安卓还不支持压缩的视频Bundle
			if (BuildTarget == BuildTarget.Android)
			{
				Log($"开始视频单独打包（安卓平台）");
				for (int i = 0; i < buildMap.Count; i++)
				{
					AssetInfo assetInfo = buildMap[i];
					if (assetInfo.IsVideoAsset)
					{
						BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
						opt |= BuildAssetBundleOptions.DeterministicAssetBundle;
						opt |= BuildAssetBundleOptions.StrictMode;
						opt |= BuildAssetBundleOptions.UncompressedAssetBundle;
						var videoObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(assetInfo.AssetPath);
						string outPath = OutputPath + "/" + assetInfo.AssetBundleLabel.ToLower();
						bool result = BuildPipeline.BuildAssetBundle(videoObj, new[] { videoObj }, outPath, opt, BuildTarget);
						if (result == false)
							throw new Exception($"视频单独打包失败：{assetInfo.AssetPath}");
					}
				}
			}
		}
		#endregion

		#region 文件加密
		private void EncryptFiles(AssetBundleManifest unityManifest)
		{
			Log($"开始加密资源文件");

			// 初始化加密器
			InitAssetEncrypter();

			int progressBarCount = 0;
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();
			foreach (string assetName in allAssetBundles)
			{
				string path = $"{OutputPath}/{assetName}";
				if (AssetEncrypterCheck(path))
				{
					byte[] fileData = File.ReadAllBytes(path);

					// 通过判断文件合法性，规避重复加密一个文件。
					if (EditorTools.CheckBundleFileValid(fileData))
					{
						byte[] bytes = AssetEncrypterEncrypt(fileData);
						File.WriteAllBytes(path, bytes);
						Log($"文件加密完成：{path}");
					}
				}

				// 进度条
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"加密资源包：{progressBarCount}/{allAssetBundles.Length}", (float)progressBarCount / allAssetBundles.Length);
			}
			EditorUtility.ClearProgressBar();
		}

		private Type _encrypterType = null;
		private void InitAssetEncrypter()
		{
			_encrypterType = Type.GetType("AssetEncrypter");
		}
		private bool AssetEncrypterCheck(string filePath)
		{
			if (_encrypterType != null)
			{
				var method = _encrypterType.GetMethod("Check");
				return (bool)method.Invoke(null, new object[] { filePath });
			}
			else
			{
				return false;
			}
		}
		private byte[] AssetEncrypterEncrypt(byte[] data)
		{
			if (_encrypterType != null)
			{
				var method = _encrypterType.GetMethod("Encrypt");
				return (byte[])method.Invoke(null, new object[] { data });
			}
			else
			{
				return data;
			}
		}
		#endregion

		#region 文件相关
		/// <summary>
		/// 1. 创建补丁清单文件到输出目录
		/// </summary>
		private void CreatePatchManifestFile(AssetBundleManifest unityManifest)
		{
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();

			// 加载旧补丁清单
			PatchManifest oldPatchManifest = LoadPatchManifestFile();

			// 创建新补丁清单
			PatchManifest newPatchManifest = new PatchManifest();

			// 写入版本信息
			newPatchManifest.ResourceVersion = BuildVersion;

			// 写入所有AssetBundle文件的信息
			for (int i = 0; i < allAssetBundles.Length; i++)
			{
				string assetName = allAssetBundles[i];
				string path = $"{OutputPath}/{assetName}";
				string md5 = HashUtility.FileMD5(path);
				long sizeBytes = EditorTools.GetFileSize(path);
				int version = BuildVersion;

				// 获取依赖列表
				string[] depend = unityManifest.GetDirectDependencies(assetName);

				// 注意：如果文件没有变化使用旧版本号
				if (oldPatchManifest.Elements.TryGetValue(assetName, out PatchElement oldElement))
				{
					if (oldElement.MD5 == md5)
						version = oldElement.Version;
				}

				PatchElement newElement = new PatchElement(assetName, md5, sizeBytes, version, depend);
				newPatchManifest.ElementList.Add(newElement);
			}

			// 写入所有变体信息
			{
				Dictionary<string, List<string>> variantInfos = GetVariantInfos(allAssetBundles);
				foreach (var pair in variantInfos)
				{
					if (pair.Value.Count > 0)
					{
						string name = $"{pair.Key}.{ PatchDefine.AssetBundleDefaultVariant}";
						List<string> variants = pair.Value;
						newPatchManifest.VariantList.Add(new PatchVariant(name, variants));
					}
				}
			}

			// 创建新文件
			string filePath = OutputPath + $"/{PatchDefine.PatchManifestFileName}";
			Log($"创建补丁清单文件：{filePath}");
			PatchManifest.Serialize(filePath, newPatchManifest);
		}
		private Dictionary<string, List<string>> GetVariantInfos(string[] allAssetBundles)
		{
			Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
			foreach (var assetName in allAssetBundles)
			{
				string path = assetName.Remove(assetName.LastIndexOf(".")); // "assets/config/test.unity3d" --> "assets/config/test"
				string extension = Path.GetExtension(assetName).Substring(1);

				if (dic.ContainsKey(path) == false)
					dic.Add(path, new List<string>());

				if (extension != PatchDefine.AssetBundleDefaultVariant)
					dic[path].Add(extension);
			}
			return dic;
		}

		/// <summary>
		/// 2. 创建Readme文件到输出目录
		/// </summary>
		private void CreateReadmeFile(AssetBundleManifest unityManifest)
		{
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();
			
			// 删除旧文件
			string filePath = OutputPath + "/readme.txt";
			if (File.Exists(filePath))
				File.Delete(filePath);

			Log($"创建说明文件：{filePath}");

			StringBuilder content = new StringBuilder();
			AppendData(content, $"构建平台：{BuildTarget}");
			AppendData(content, $"构建版本：{BuildVersion}");
			AppendData(content, $"构建时间：{DateTime.Now}");

			AppendData(content, "");
			AppendData(content, $"--配置信息--");
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Elements.Count; i++)
			{
				AssetBundleCollectorSetting.Wrapper wrapper = AssetBundleCollectorSettingData.Setting.Elements[i];
				AppendData(content, $"FolderPath : {wrapper.FolderPath} || PackRule : {wrapper.PackRule} || LabelRule : {wrapper.LabelRule}");
			}

			AppendData(content, "");
			AppendData(content, $"--构建参数--");
			AppendData(content, $"CompressOption：{CompressOption}");
			AppendData(content, $"ForceRebuild：{IsForceRebuild}");
			AppendData(content, $"DisableWriteTypeTree：{IsDisableWriteTypeTree}");
			AppendData(content, $"IgnoreTypeTreeChanges：{IsIgnoreTypeTreeChanges}");

			AppendData(content, "");
			AppendData(content, $"--构建清单--");
			for (int i = 0; i < allAssetBundles.Length; i++)
			{
				AppendData(content, allAssetBundles[i]);
			}

			PatchManifest patchFile = LoadPatchManifestFile();
			{
				AppendData(content, "");
				AppendData(content, $"--更新清单--");
				foreach (var element in patchFile.ElementList)
				{
					if (element.Version == BuildVersion)
					{
						AppendData(content, element.Name);
					}
				}

				AppendData(content, "");
				AppendData(content, $"--变体列表--");
				foreach (var variant in patchFile.VariantList)
				{
					AppendData(content, variant.ToString());
				}
			}

			// 创建新文件
			File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
		}
		private void AppendData(StringBuilder sb, string data)
		{
			sb.Append(data);
			sb.Append("\r\n");
		}

		/// <summary>
		/// 3. 复制更新文件到补丁包目录
		/// </summary>
		private void CopyUpdateFiles()
		{
			string packageFolderPath = GetPackageFolderPath();
			Log($"开始复制更新文件到补丁包目录：{packageFolderPath}");

			// 复制Readme文件
			{
				string sourcePath = $"{OutputPath}/readme.txt";
				string destPath = $"{packageFolderPath}/readme.txt";
				EditorTools.CopyFile(sourcePath, destPath, true);
				Log($"复制Readme文件到：{destPath}");
			}

			// 复制PatchManifest文件
			{
				string sourcePath = $"{OutputPath}/{PatchDefine.PatchManifestFileName}";
				string destPath = $"{packageFolderPath}/{PatchDefine.PatchManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				Log($"复制PatchManifest文件到：{destPath}");
			}

			// 复制UnityManifest文件
			{
				string sourcePath = $"{OutputPath}/{PatchDefine.UnityManifestFileName}";
				string destPath = $"{packageFolderPath}/{PatchDefine.UnityManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				Log($"复制UnityManifest文件到：{destPath}");
			}

			// 复制Manifest文件
			{
				string sourcePath = $"{OutputPath}/{PatchDefine.UnityManifestFileName}.manifest";
				string destPath = $"{packageFolderPath}/{PatchDefine.UnityManifestFileName}.manifest";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 复制所有更新文件
			PatchManifest patchFile = LoadPatchManifestFile();
			foreach (var pair in patchFile.Elements)
			{
				if (pair.Value.Version == BuildVersion)
				{
					string sourcePath = $"{OutputPath}/{pair.Key}";
					string destPath = $"{packageFolderPath}/{pair.Key}";
					EditorTools.CopyFile(sourcePath, destPath, true);
					Log($"复制更新文件：{destPath}");
				}
			}
		}

		/// <summary>
		/// 从输出目录加载补丁清单文件
		/// </summary>
		private PatchManifest LoadPatchManifestFile()
		{
			string filePath = $"{OutputPath}/{PatchDefine.PatchManifestFileName}";
			if (File.Exists(filePath) == false)
				return new PatchManifest();

			string jsonData = FileUtility.ReadFile(filePath);
			return PatchManifest.Deserialize(jsonData);
		}
		#endregion
	}
}