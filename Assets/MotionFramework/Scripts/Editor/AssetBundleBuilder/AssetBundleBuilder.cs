//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
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
		public string OutputDirectory { private set; get; } = string.Empty;

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
			OutputDirectory = GetOutputDirectory();
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
			if (string.IsNullOrEmpty(OutputDirectory))
				throw new Exception("[BuildPatch] 输出目录不能为空");

			// 检测补丁包是否已经存在
			string packageDirectory = GetPackageDirectory();
			if (Directory.Exists(packageDirectory))
				throw new Exception($"[BuildPatch] 补丁包已经存在：{packageDirectory}");

			// 如果是强制重建
			if (IsForceRebuild)
			{
				// 删除平台总目录
				string platformDirectory = $"{_outputRoot}/{BuildTarget}";
				if (Directory.Exists(platformDirectory))
				{
					Directory.Delete(platformDirectory, true);
					Log($"删除平台总目录：{platformDirectory}");
				}
			}

			// 如果输出目录不存在
			if (Directory.Exists(OutputDirectory) == false)
			{
				Directory.CreateDirectory(OutputDirectory);
				Log($"创建输出目录：{OutputDirectory}");
			}
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		public void PostAssetBuild()
		{
			Debug.Log("------------------------------OnPostAssetBuild------------------------------");

			// 检测工作
			if (AssetBundleCollectorSettingData.GetCollecterCount() == 0)
				throw new Exception("[BuildPatch] 配置的资源收集路径为空！");

			// 准备工作	
			List<AssetInfo> buildMap = GetBuildMap();
			if (buildMap.Count == 0)
				throw new Exception("[BuildPatch] 构建列表不能为空");

			Log($"构建列表里总共有{buildMap.Count}个资源需要构建");
			List<AssetBundleBuild> buildInfoList = new List<AssetBundleBuild>(buildMap.Count);
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
			AssetBundleManifest unityManifest = BuildPipeline.BuildAssetBundles(OutputDirectory, buildInfoList.ToArray(), opt, BuildTarget);
			if (unityManifest == null)
				throw new Exception("[BuildPatch] 构建过程中发生错误！");

			// 加密资源文件
			List<string> encryptList = EncryptFiles(unityManifest);

			// 1. 检测循环依赖
			CheckCycleDepend(unityManifest);
			// 2. 创建补丁文件
			CreatePatchManifestFile(unityManifest, buildMap, encryptList);
			// 3. 创建说明文件
			CreateReadmeFile(unityManifest);
			// 4. 复制更新文件
			CopyUpdateFiles();

			Log("构建完成！");
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
		private string GetOutputDirectory()
		{
			return $"{_outputRoot}/{BuildTarget}/{PatchDefine.UnityManifestFileName}";
		}
		private string GetPackageDirectory()
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
			Dictionary<string, AssetInfo> buildMap = new Dictionary<string, AssetInfo>();

			// 获取要收集的资源
			List<string> allCollectAssets = AssetBundleCollectorSettingData.GetAllCollectAssets();

			// 进行依赖分析
			foreach (string mainAssetPath in allCollectAssets)
			{
				List<AssetInfo> depends = GetDependencies(mainAssetPath);
				for (int i = 0; i < depends.Count; i++)
				{
					AssetInfo assetInfo = depends[i];
					if (buildMap.ContainsKey(assetInfo.AssetPath))
						buildMap[assetInfo.AssetPath].DependCount++;
					else
						buildMap.Add(assetInfo.AssetPath, assetInfo);
				}
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"依赖文件分析：{progressBarCount}/{allCollectAssets.Count}", (float)progressBarCount / allCollectAssets.Count);
			}
			EditorUtility.ClearProgressBar();
			progressBarCount = 0;

			// 移除零依赖的资源
			List<string> removeList = new List<string>();
			foreach (KeyValuePair<string, AssetInfo> pair in buildMap)
			{
				if (pair.Value.IsCollectAsset)
					continue;
				if (pair.Value.DependCount == 0)
					removeList.Add(pair.Value.AssetPath);
			}
			for (int i = 0; i < removeList.Count; i++)
			{
				buildMap.Remove(removeList[i]);
			}

			// 设置资源标签
			foreach (KeyValuePair<string, AssetInfo> pair in buildMap)
			{
				var assetInfo = pair.Value;
				var labelAndVariant = AssetBundleCollectorSettingData.GetBundleLabelAndVariant(assetInfo.AssetPath, assetInfo.AssetType);
				assetInfo.AssetBundleLabel = labelAndVariant.BundleLabel;
				assetInfo.AssetBundleVariant = labelAndVariant.BundleVariant;
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"设置资源标签：{progressBarCount}/{buildMap.Count}", (float)progressBarCount / buildMap.Count);
			}
			EditorUtility.ClearProgressBar();

			// 返回结果
			return buildMap.Values.ToList();
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
				if (AssetBundleCollectorSettingData.ValidateAsset(dependPath))
				{
					AssetInfo assetInfo = new AssetInfo(dependPath);
					depends.Add(assetInfo);
				}
			}
			return depends;
		}
		#endregion

		#region 文件加密
		private IAssetEncrypter _encrypter = null;
		private void InitAssetEncrypter()
		{
			var types = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IAssetEncrypter));
			if (types.Count == 0)
				return;
			if (types.Count != 1)
			{
				Debug.LogError($"Found more {nameof(IAssetEncrypter)} types. We only support one.");
				return;
			}

			Log($"创建加密类 : {types[0].FullName}");
			_encrypter = (IAssetEncrypter)Activator.CreateInstance(types[0]);
		}

		private List<string> EncryptFiles(AssetBundleManifest unityManifest)
		{
			// 初始化加密器
			InitAssetEncrypter();

			// 加密资源列表
			List<string> encryptList = new List<string>();

			// 如果没有设置加密类
			if (_encrypter == null)
				return encryptList;

			Log($"开始加密资源文件");
			int progressBarCount = 0;
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();
			foreach (string assetName in allAssetBundles)
			{
				string filePath = $"{OutputDirectory}/{assetName}";
				if (_encrypter.Check(filePath))
				{
					encryptList.Add(assetName);

					// 通过判断文件合法性，规避重复加密一个文件
					byte[] fileData = File.ReadAllBytes(filePath);
					if (EditorTools.CheckBundleFileValid(fileData))
					{
						byte[] bytes = _encrypter.Encrypt(fileData);
						File.WriteAllBytes(filePath, bytes);
						Log($"文件加密完成：{filePath}");
					}
				}

				// 进度条
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"加密资源包：{progressBarCount}/{allAssetBundles.Length}", (float)progressBarCount / allAssetBundles.Length);
			}
			EditorUtility.ClearProgressBar();

			return encryptList;
		}
		#endregion

		#region 文件相关
		/// <summary>
		/// 1. 检测循环依赖
		/// </summary>
		private void CheckCycleDepend(AssetBundleManifest unityManifest)
		{
			List<string> visited = new List<string>(100);
			List<string> stack = new List<string>(100);
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();
			for (int i = 0; i < allAssetBundles.Length; i++)
			{
				var element = allAssetBundles[i];
				visited.Clear();
				stack.Clear();

				// 深度优先搜索检测有向图有无环路算法
				if (CheckCycle(unityManifest, element, visited, stack))
				{
					foreach (var ele in stack)
					{
						UnityEngine.Debug.LogWarning(ele);
					}
					throw new Exception($"Found cycle assetbundle : {element}");
				}
			}
		}
		private bool CheckCycle(AssetBundleManifest unityManifest, string element, List<string> visited, List<string> stack)
		{
			if (visited.Contains(element) == false)
			{
				visited.Add(element);
				stack.Add(element);

				string[] depends = unityManifest.GetDirectDependencies(element);
				foreach (var dp in depends)
				{
					if (visited.Contains(dp) == false && CheckCycle(unityManifest, dp, visited, stack))
						return true;
					else if (stack.Contains(dp))
						return true;
				}
			}

			stack.Remove(element);
			return false;
		}

		/// <summary>
		/// 2. 创建补丁清单文件到输出目录
		/// </summary>
		private void CreatePatchManifestFile(AssetBundleManifest unityManifest, List<AssetInfo> buildMap, List<string> encryptList)
		{
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();

			// 创建DLC管理器
			DLCManager dlcManager = new DLCManager();
			dlcManager.LoadAllDLC();

			// 加载旧补丁清单
			PatchManifest oldPatchManifest = LoadPatchManifestFile();

			// 创建新补丁清单
			PatchManifest newPatchManifest = new PatchManifest();

			// 写入版本信息
			newPatchManifest.ResourceVersion = BuildVersion;

			// 写入所有AssetBundle文件的信息
			for (int i = 0; i < allAssetBundles.Length; i++)
			{
				string bundleName = allAssetBundles[i];
				string path = $"{OutputDirectory}/{bundleName}";
				string md5 = HashUtility.FileMD5(path);
				uint crc32 = HashUtility.FileCRC32(path);
				long sizeBytes = EditorTools.GetFileSize(path);
				int version = BuildVersion;
				string[] assetPaths = GetBundleAssetPaths(buildMap, bundleName);
				string[] depends = unityManifest.GetDirectDependencies(bundleName);
				string[] dlcLabels = dlcManager.GetAssetBundleDLCLabels(bundleName);

				// 创建标记位
				bool isEncrypted = encryptList.Contains(bundleName);
				bool isCollected = IsCollectBundle(buildMap, bundleName);
				int flags = PatchElement.CreateFlags(isEncrypted, isCollected);

				// 注意：如果文件没有变化使用旧版本号
				if (oldPatchManifest.Elements.TryGetValue(bundleName, out PatchElement oldElement))
				{
					if (oldElement.MD5 == md5)
						version = oldElement.Version;
				}

				PatchElement newElement = new PatchElement(bundleName, md5, crc32, sizeBytes, version, flags, assetPaths, depends, dlcLabels);
				newPatchManifest.ElementList.Add(newElement);
			}

			// 写入所有变体信息
			{
				Dictionary<string, List<string>> variantInfos = GetVariantInfos(allAssetBundles);
				foreach (var pair in variantInfos)
				{
					if (pair.Value.Count > 0)
					{
						string bundleName = $"{pair.Key}.{ PatchDefine.AssetBundleDefaultVariant}";
						List<string> variants = pair.Value;
						newPatchManifest.VariantList.Add(new PatchVariant(bundleName, variants));
					}
				}
			}

			// 创建新文件
			string filePath = OutputDirectory + $"/{PatchDefine.PatchManifestFileName}";
			Log($"创建补丁清单文件：{filePath}");
			PatchManifest.Serialize(filePath, newPatchManifest);
		}
		private string[] GetBundleAssetPaths(List<AssetInfo> buildMap, string assetBundleLabel)
		{
			List<string> result = new List<string>();
			for (int i = 0; i < buildMap.Count; i++)
			{
				AssetInfo assetInfo = buildMap[i];
				string label = $"{assetInfo.AssetBundleLabel}.{assetInfo.AssetBundleVariant}".ToLower();
				if (label == assetBundleLabel)
				{
					//string assetPath = assetInfo.AssetPath.Remove(assetInfo.AssetPath.LastIndexOf(".")); // "assets/config/test.unity3d" --> "assets/config/test"
					result.Add(assetInfo.AssetPath.ToLower());
				}
			}
			return result.ToArray();
		}
		private Dictionary<string, List<string>> GetVariantInfos(string[] allAssetBundles)
		{
			Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
			foreach (var assetBundleLabel in allAssetBundles)
			{
				string path = assetBundleLabel.Remove(assetBundleLabel.LastIndexOf(".")); // "assets/config/test.unity3d" --> "assets/config/test"
				string extension = Path.GetExtension(assetBundleLabel).Substring(1);

				if (dic.ContainsKey(path) == false)
					dic.Add(path, new List<string>());

				if (extension != PatchDefine.AssetBundleDefaultVariant)
					dic[path].Add(extension);
			}
			return dic;
		}
		private bool IsCollectBundle(List<AssetInfo> buildMap, string assetBundleLabel)
		{
			for (int i = 0; i < buildMap.Count; i++)
			{
				AssetInfo assetInfo = buildMap[i];
				string label = $"{assetInfo.AssetBundleLabel}.{assetInfo.AssetBundleVariant}".ToLower();
				if (label == assetBundleLabel)
				{
					if (assetInfo.IsCollectAsset)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 3. 创建Readme文件到输出目录
		/// </summary>
		private void CreateReadmeFile(AssetBundleManifest unityManifest)
		{
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();

			// 删除旧文件
			string filePath = $"{OutputDirectory}/{PatchDefine.ReadmeFileName}";
			if (File.Exists(filePath))
				File.Delete(filePath);

			Log($"创建说明文件：{filePath}");

			StringBuilder content = new StringBuilder();
			AppendData(content, $"构建平台：{BuildTarget}");
			AppendData(content, $"构建版本：{BuildVersion}");
			AppendData(content, $"构建时间：{DateTime.Now}");

			AppendData(content, "");
			AppendData(content, $"--配置信息--");
			for (int i = 0; i < AssetBundleCollectorSettingData.Setting.Collectors.Count; i++)
			{
				AssetBundleCollectorSetting.Collector wrapper = AssetBundleCollectorSettingData.Setting.Collectors[i];
				AppendData(content, $"Directory : {wrapper.CollectDirectory} | {wrapper.LabelClassName} | {wrapper.FilterClassName}");
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
						AppendData(content, element.BundleName);
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
		/// 4. 复制更新文件到补丁包目录
		/// </summary>
		private void CopyUpdateFiles()
		{
			string packageDirectory = GetPackageDirectory();
			Log($"开始复制更新文件到补丁包目录：{packageDirectory}");

			// 复制Readme文件
			{
				string sourcePath = $"{OutputDirectory}/{PatchDefine.ReadmeFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.ReadmeFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				Log($"复制Readme文件到：{destPath}");
			}

			// 复制PatchManifest文件
			{
				string sourcePath = $"{OutputDirectory}/{PatchDefine.PatchManifestFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.PatchManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				Log($"复制PatchManifest文件到：{destPath}");
			}

			// 复制UnityManifest文件
			{
				string sourcePath = $"{OutputDirectory}/{PatchDefine.UnityManifestFileName}";
				string destPath = $"{packageDirectory}/{PatchDefine.UnityManifestFileName}";
				EditorTools.CopyFile(sourcePath, destPath, true);
				Log($"复制UnityManifest文件到：{destPath}");
			}

			// 复制Manifest文件
			{
				string sourcePath = $"{OutputDirectory}/{PatchDefine.UnityManifestFileName}.manifest";
				string destPath = $"{packageDirectory}/{PatchDefine.UnityManifestFileName}.manifest";
				EditorTools.CopyFile(sourcePath, destPath, true);
			}

			// 复制所有更新文件
			int progressBarCount = 0;
			PatchManifest patchFile = LoadPatchManifestFile();
			foreach (var element in patchFile.ElementList)
			{
				if (element.Version == BuildVersion)
				{
					string sourcePath = $"{OutputDirectory}/{element.BundleName}";
					string destPath = $"{packageDirectory}/{element.MD5}";
					EditorTools.CopyFile(sourcePath, destPath, true);
					Log($"复制更新文件到补丁包：{sourcePath}");

					progressBarCount++;
					EditorUtility.DisplayProgressBar("进度", $"拷贝更新文件 : {sourcePath}", (float)progressBarCount / patchFile.ElementList.Count);
				}
			}
			EditorUtility.ClearProgressBar();
		}

		/// <summary>
		/// 从输出目录加载补丁清单文件
		/// </summary>
		private PatchManifest LoadPatchManifestFile()
		{
			string filePath = $"{OutputDirectory}/{PatchDefine.PatchManifestFileName}";
			if (File.Exists(filePath) == false)
				return new PatchManifest();

			string jsonData = FileUtility.ReadFile(filePath);
			return PatchManifest.Deserialize(jsonData);
		}
		#endregion
	}
}