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
	public static class CollectorConfigImporter
	{
		private class CollectWrapper
		{
			public string CollectDirectory;
			public string BundleLabelClassName;
			public string SearchFilterClassName;
			public CollectWrapper(string directory, string labelClassName, string filterClassName)
			{
				CollectDirectory = directory;
				BundleLabelClassName = labelClassName;
				SearchFilterClassName = filterClassName;
			}
		}

		public const string XmlTag = "Collect";
		public const string XmlDirectory = "Directory";
		public const string XmlLabelClassName = "LabelClassName";
		public const string XmlFilterClassName = "FilterClassName";

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

				if (Directory.Exists(directory) == false)
					throw new Exception($"Not found directory : {directory}");
				if (string.IsNullOrEmpty(labelClassName))
					throw new Exception($"Not found attribute {XmlLabelClassName} in collector : {directory}");
				if (string.IsNullOrEmpty(filterClassName))
					throw new Exception($"Not found attribute {XmlFilterClassName} in collector : {directory}");
				if (AssetBundleCollectorSettingData.HasBundleLabelClassName(labelClassName) == false)
					throw new Exception($"Not found BundleLabelClassName : {labelClassName}");
				if (AssetBundleCollectorSettingData.HasSearchFilterClassName(filterClassName) == false)
					throw new Exception($"Not found SearchFilterClassName : {filterClassName}");

				var collectWrapper = new CollectWrapper(directory, labelClassName, filterClassName);
				wrappers.Add(collectWrapper);
			}

			// 导入配置数据
			AssetBundleCollectorSettingData.ClearAllCollector();
			foreach (var wrapper in wrappers)
			{
				AssetBundleCollectorSettingData.AddCollector(wrapper.CollectDirectory, wrapper.BundleLabelClassName, wrapper.SearchFilterClassName, false);
			}
			AssetBundleCollectorSettingData.SaveFile();
			Debug.Log($"导入配置完毕，一共导入{wrappers.Count}个收集器。");
		}
	}
}