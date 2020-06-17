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
		private static readonly Dictionary<string, Type> _cfgTypes = new Dictionary<string, Type>();
		private static string _configAssemblyName;

		public static void Initialize(string assemblyName)
		{
			if (string.IsNullOrEmpty(assemblyName))
				throw new ArgumentNullException();
			_configAssemblyName = assemblyName;

			List<Type> result = AssemblyUtility.GetAssignableAttributeTypes(assemblyName, typeof(AssetConfig), typeof(ConfigAttribute));
			for (int i = 0; i < result.Count; i++)
			{
				Type type = result[i];

				// 判断是否重复
				ConfigAttribute attribute = (ConfigAttribute)Attribute.GetCustomAttribute(type, typeof(ConfigAttribute));
				if (_cfgTypes.ContainsKey(attribute.CfgName))
					throw new Exception($"Config {type} has same attribute value : {attribute.CfgName}");

				// 添加到集合
				_cfgTypes.Add(attribute.CfgName, type);
			}
		}
		public static AssetConfig CreateInstance(string cfgName)
		{
			AssetConfig config = null;

			if (_cfgTypes.TryGetValue(cfgName, out Type type))
				config = (AssetConfig)Activator.CreateInstance(type);

			if (config == null)
				throw new KeyNotFoundException($"{nameof(AssetConfig)} {cfgName} is not define in assembly {_configAssemblyName}");

			return config;
		}
	}
}