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
using MotionFramework.IO;

namespace MotionFramework.Editor
{
	public static class CollectorConfigImporter
	{
		private class CollectWrapper
		{
			public string CollectDirectory;
			public string PackRuleClassName;
			public string FilterRuleClassName;
			public bool DontWriteAssetPath;
			public CollectWrapper(string directory, string packRuleClassName, string filterRuleClassName, bool dontWriteAssetPath)
			{
				CollectDirectory = directory;
				PackRuleClassName = packRuleClassName;
				FilterRuleClassName = filterRuleClassName;
				DontWriteAssetPath = dontWriteAssetPath;
			}
		}

		public const string XmlTag = "Collector";
		public const string XmlDirectory = "Directory";
		public const string XmlPackRuleName = "PackRule";
		public const string XmlFilterRuleName = "FilterRule";
		public const string XmlDontWriteAssetPath = "DontWriteAssetPath";
		
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
			if (nodeList.Count == 0)
				throw new Exception($"Not found any {XmlTag}");
			foreach (XmlNode node in nodeList)
			{
				XmlElement collect = node as XmlElement;
				string directory = collect.GetAttribute(XmlDirectory);
				string packRuleClassName = collect.GetAttribute(XmlPackRuleName);
				string filterRuleClassName = collect.GetAttribute(XmlFilterRuleName);
				string dontWriteAssetPath = collect.GetAttribute(XmlDontWriteAssetPath);

				if (Directory.Exists(directory) == false)
					throw new Exception($"Not found directory : {directory}");
				if (string.IsNullOrEmpty(packRuleClassName))
					throw new Exception($"Not found attribute {XmlPackRuleName} in collector : {directory}");
				if (string.IsNullOrEmpty(filterRuleClassName))
					throw new Exception($"Not found attribute {XmlFilterRuleName} in collector : {directory}");
				if (string.IsNullOrEmpty(dontWriteAssetPath))
					throw new Exception($"Not found attribute {XmlDontWriteAssetPath} in collector : {directory}");
				if (AssetBundleCollectorSettingData.HasPackRuleClassName(packRuleClassName) == false)
					throw new Exception($"Invalid {nameof(IPackRule)} class type : {packRuleClassName}");
				if (AssetBundleCollectorSettingData.HasFilterRuleClassName(filterRuleClassName) == false)
					throw new Exception($"Invalid {nameof(IFilterRule)} class type : {filterRuleClassName}");

				bool dontWriteAssetPathFlag = StringConvert.StringToBool(dontWriteAssetPath);
				var collectWrapper = new CollectWrapper(directory, packRuleClassName, filterRuleClassName, dontWriteAssetPathFlag);
				wrappers.Add(collectWrapper);
			}

			// 导入配置数据
			AssetBundleCollectorSettingData.ClearAllCollector();
			foreach (var wrapper in wrappers)
			{
				AssetBundleCollectorSettingData.AddCollector(wrapper.CollectDirectory, wrapper.PackRuleClassName, wrapper.FilterRuleClassName, wrapper.DontWriteAssetPath, false);
			}
			AssetBundleCollectorSettingData.SaveFile();
			Debug.Log($"导入配置完毕，一共导入{wrappers.Count}个收集器。");
		}
	}
}