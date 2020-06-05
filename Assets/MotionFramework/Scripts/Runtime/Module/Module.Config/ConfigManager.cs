//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MotionFramework.Config
{
	/// <summary>
	/// 配表管理器
	/// </summary>
	public sealed class ConfigManager : ModuleSingleton<ConfigManager>, IModule
	{
		private Dictionary<string, AssetConfig> _configs = new Dictionary<string, AssetConfig>();


		void IModule.OnCreate(System.Object param)
		{
		}
		void IModule.OnUpdate()
		{
		}
		void IModule.OnGUI()
		{
		}

		/// <summary>
		/// 按照表格顺序异步加载所有表格
		/// </summary>
		public IEnumerator LoadConfigs(List<string> locations)
		{
			for (int i = 0; i < locations.Count; i++)
			{
				string location = locations[i];			
				AssetConfig config = LoadConfig(location);
				yield return config;
			}
		}
		
		/// <summary>
		/// 加载配表
		/// </summary>
		public AssetConfig LoadConfig(string location)
		{
			string configName = Path.GetFileName(location);

			// 防止重复加载
			if (_configs.ContainsKey(configName))
			{
				MotionLog.Log(ELogLevel.Error, $"Config {configName} is already existed.");
				return null;
			}

			AssetConfig config = ConfigCreater.CreateInstance(configName);
			if (config != null)
			{
				config.Load(location);
				_configs.Add(configName, config);
			}
			else
			{
				MotionLog.Log(ELogLevel.Error, $"Config type {configName} is invalid.");
			}
			return config;
		}

		/// <summary>
		/// 获取配表
		/// </summary>
		public AssetConfig GetConfig(string configName)
		{
			if (_configs.ContainsKey(configName))
			{
				return _configs[configName];
			}

			MotionLog.Log(ELogLevel.Error, $"Not found config {configName}");
			return null;
		}

		/// <summary>
		/// 获取配表
		/// </summary>
		public T GetConfig<T>() where T : AssetConfig
		{
			System.Type type = typeof(T);
			foreach (var pair in _configs)
			{
				if (pair.Value.GetType() == type)
					return pair.Value as T;
			}

			MotionLog.Log(ELogLevel.Error, $"Not found config {type}");
			return null;
		}
	}
}