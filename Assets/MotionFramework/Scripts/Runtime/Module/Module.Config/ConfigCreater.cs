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
	public class ConfigCreater
	{
		private static Dictionary<string, Type> _cfgTypes = new Dictionary<string, Type>();

		static ConfigCreater()
		{
			List<Type> result = AssemblyUtility.GetAssignableAttributeTypes(typeof(AssetConfig), typeof(ConfigAttribute));
			for (int i = 0; i < result.Count; i++)
			{
				Type type = result[i];

				// 判断是否重复
				ConfigAttribute attribute = (ConfigAttribute)Attribute.GetCustomAttribute(type, typeof(ConfigAttribute));
				if (_cfgTypes.ContainsKey(attribute.CfgName))
					throw new Exception($"Config {type} has same value : {attribute.CfgName}");

				// 添加到集合
				_cfgTypes.Add(attribute.CfgName, type);
			}
		}

		public static AssetConfig CreateInstance(string cfgName)
		{
			AssetConfig config = null;

			Type type;
			if (_cfgTypes.TryGetValue(cfgName, out type))
				config = (AssetConfig)Activator.CreateInstance(type);

			if (config == null)
				throw new KeyNotFoundException($"AssetConfig {cfgName} is not define.");

			return config;
		}
	}
}