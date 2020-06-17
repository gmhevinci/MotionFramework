//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Utility;

namespace MotionFramework.Config
{
	internal class ConfigCreater
	{
		private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
		private string _assemblyName;

		/// <summary>
		/// 初始化
		/// </summary>
		public void Initialize(List<Type> allTypes, string assemblyName)
		{
			if (allTypes == null || allTypes.Count == 0)
			{
				if (string.IsNullOrEmpty(assemblyName))
					throw new ArgumentNullException();
				_assemblyName = assemblyName;
				allTypes = AssemblyUtility.GetAssignableAttributeTypes(assemblyName, typeof(AssetConfig), typeof(ConfigAttribute));
			}

			for (int i = 0; i < allTypes.Count; i++)
			{
				Type type = allTypes[i];

				// 判断是否重复
				ConfigAttribute attribute = (ConfigAttribute)Attribute.GetCustomAttribute(type, typeof(ConfigAttribute));
				if (_types.ContainsKey(attribute.CfgName))
					throw new Exception($"Config {type} has same attribute value : {attribute.CfgName}");

				// 添加到集合
				_types.Add(attribute.CfgName, type);
			}
		}

		/// <summary>
		/// 创建类的实例
		/// </summary>
		/// <param name="cfgName">配表名称</param>
		public AssetConfig CreateInstance(string cfgName)
		{
			AssetConfig config = null;

			if (_types.TryGetValue(cfgName, out Type type))
				config = (AssetConfig)Activator.CreateInstance(type);

			if (config == null)
			{
				if(string.IsNullOrEmpty(_assemblyName) == false)
					throw new KeyNotFoundException($"{nameof(AssetConfig)} {cfgName} is not define in assembly {_assemblyName}");
				else
					throw new KeyNotFoundException($"{nameof(AssetConfig)} {cfgName} is create failed.");
			}

			return config;
		}
	}
}