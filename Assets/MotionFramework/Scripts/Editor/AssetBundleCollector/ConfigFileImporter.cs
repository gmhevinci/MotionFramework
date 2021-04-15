//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MotionFramework.Editor
{
	public static class ConfigFileImporter
	{
		public const string XmlTag = "Collect";
		public const string XmlDirectory = "Directory";
		public const string XmlLabelClassName = "LabelClassName";
		public const string XmlFilterClassName = "FilterClassName";

		private class CollectWrapper
		{
			public string CollectDirectory;
			public string BundleLabelClassName;
			public string SearchFilterClassName;
			public CollectWrapper(string directory, string labelClassName, string filterClassName)
			{
				// 注意：路径末尾一定要文件分隔符
				if (directory.EndsWith("/") == false)
					directory = $"{directory}/";

				CollectDirectory = directory;
				BundleLabelClassName = labelClassName;
				SearchFilterClassName = filterClassName;
			}
			public void CheckInvalid()
			{
				if (Directory.Exists(CollectDirectory) == false)
					throw new Exception($"Not found directory : {CollectDirectory}");

				if (string.IsNullOrEmpty(BundleLabelClassName))
					throw new Exception($"{CollectDirectory} {nameof(BundleLabelClassName)} is invalid. Check config file use : {XmlLabelClassName}");
				if (string.IsNullOrEmpty(SearchFilterClassName))
					throw new Exception($"{CollectDirectory} {nameof(SearchFilterClassName)} is invalid. Check config file use : {XmlFilterClassName}");

				if (AssetBundleCollectorSettingData.HasBundleLabelClassName(BundleLabelClassName) == false)
					throw new Exception($"Not found BundleLabelClassName : {BundleLabelClassName}");
				if (AssetBundleCollectorSettingData.HasSearchFilterClassName(SearchFilterClassName) == false)
					throw new Exception($"Not found SearchFilterClassName : {SearchFilterClassName}");
			}
			public override string ToString()
			{
				return $"CollectDirectory : {CollectDirectory}  BundleLabelClassName : {BundleLabelClassName}  SearchFilterClassName : {SearchFilterClassName}";
			}
		}

		public static void ImportXmlConfig(string filePath)
		{
			if (File.Exists(filePath) == false)
				throw new FileNotFoundException(filePath);

			if (Path.GetExtension(filePath) != ".xml")
				throw new Exception($"Only support xml : {filePath}");

			List<CollectWrapper> wrappers = new List<CollectWrapper>();

			// 加载文件
			XmlDocument xml = new XmlDocument();
			xml.Load(filePath);

			// 解析文件
			XmlElement root = xml.DocumentElement;
			XmlNodeList nodeList = root.GetElementsByTagName(XmlTag);
			foreach (XmlNode node in nodeList)
			{
				XmlElement collect = node as XmlElement;
				string directory = collect.GetAttribute(XmlDirectory);
				string labelClassName = collect.GetAttribute(XmlLabelClassName);
				string filterClassName = collect.GetAttribute(XmlFilterClassName);
				var collectWrapper = new CollectWrapper(directory, labelClassName, filterClassName);
				collectWrapper.CheckInvalid();
				wrappers.Add(collectWrapper);
			}

			// 导入配置数据
			AssetBundleCollectorSettingData.ClearAllCollector();
			foreach(var wrapper in wrappers)
			{
				AssetBundleCollectorSettingData.AddCollector(wrapper.CollectDirectory);
				AssetBundleCollectorSettingData.ModifyCollector(wrapper.CollectDirectory, wrapper.BundleLabelClassName, wrapper.SearchFilterClassName);
				Debug.Log($"Import : {wrapper}");
			}
			Debug.Log($"导入配置完毕，一共导入{wrappers.Count}个收集器。");
		}
	}
}