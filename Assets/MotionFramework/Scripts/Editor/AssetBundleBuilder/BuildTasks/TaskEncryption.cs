//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MotionFramework.Utility;

namespace MotionFramework.Editor
{
	internal class TaskEncryption : IBuildTask
	{
		public class EncryptionContext : IContextObject
		{
			public List<string> EncryptList;
		}

		private IAssetEncrypter _encrypter = null;

		void IBuildTask.Run(BuildContext context)
		{
			var buildParameters = context.GetContextObject<AssetBundleBuilder.BuildParametersContext>();

			// 初始化加密器
			InitAssetEncrypter();

			var unityManifestContext = context.GetContextObject<TaskBuilding.UnityManifestContext>();
			List<string> encryptList = EncryptFiles(unityManifestContext.Manifest, buildParameters);

			EncryptionContext encryptionContext = new EncryptionContext();
			encryptionContext.EncryptList = encryptList;
			context.SetContextObject(encryptionContext);
		}

		private void InitAssetEncrypter()
		{
			var types = AssemblyUtility.GetAssignableTypes(AssemblyUtility.UnityDefaultAssemblyEditorName, typeof(IAssetEncrypter));
			if (types.Count == 0)
				return;
			if (types.Count != 1)
				throw new Exception($"Found more {nameof(IAssetEncrypter)} types. We only support one.");

			BuildLogger.Log($"创建加密类 : {types[0].FullName}");
			_encrypter = (IAssetEncrypter)Activator.CreateInstance(types[0]);
		}
		private List<string> EncryptFiles(AssetBundleManifest unityManifest, AssetBundleBuilder.BuildParametersContext buildParameters)
		{
			// 加密资源列表
			List<string> encryptList = new List<string>();

			// 如果没有设置加密类
			if (_encrypter == null)
				return encryptList;

			BuildLogger.Log($"开始加密资源文件");
			int progressBarCount = 0;
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();
			foreach (string assetName in allAssetBundles)
			{
				string filePath = $"{buildParameters.OutputDirectory}/{assetName}";
				if (_encrypter.Check(filePath))
				{
					encryptList.Add(assetName);

					// 通过判断文件合法性，规避重复加密一个文件
					byte[] fileData = File.ReadAllBytes(filePath);
					if (EditorTools.CheckBundleFileValid(fileData))
					{
						byte[] bytes = _encrypter.Encrypt(fileData);
						File.WriteAllBytes(filePath, bytes);
						BuildLogger.Log($"文件加密完成：{filePath}");
					}
				}

				// 进度条
				progressBarCount++;
				EditorUtility.DisplayProgressBar("进度", $"加密资源包：{progressBarCount}/{allAssetBundles.Length}", (float)progressBarCount / allAssetBundles.Length);
			}
			EditorUtility.ClearProgressBar();

			return encryptList;
		}
	}
}