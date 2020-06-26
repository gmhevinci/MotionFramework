//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Config
{
	/// <summary>
	/// 配表管理器
	/// </summary>
	public sealed class ConfigManager : ModuleSingleton<ConfigManager>, IModule
	{
		public class LoadPair
		{
			public Type ClassType;
			public string Location;
			public LoadPair(Type classType, string location)
			{
				ClassType = classType;
				Location = location;
			}
		}

		private readonly Dictionary<string, AssetConfig> _configs = new Dictionary<string, AssetConfig>();

		/// <summary>
		/// 反射服务接口
		/// </summary>
		public IActivatorServices ActivatorServices { get; set; }

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
		/// 按照列表顺序批量加载配表
		/// </summary>
		public IEnumerator LoadConfigs(List<LoadPair> loadPairs)
		{
			for(int i=0; i< loadPairs.Count; i++)
			{
				Type type = loadPairs[i].ClassType;
				string location = loadPairs[i].Location;
				AssetConfig config = LoadConfig(type, location);
				yield return config;
			}
		}

		/// <summary>
		/// 加载配表
		/// </summary>
		public T LoadConfig<T>(string location) where T : AssetConfig
		{
			return LoadConfig(typeof(T), location) as T;
		}
		public AssetConfig LoadConfig(Type configType, string location)
		{
			string configName = configType.FullName;

			// 防止重复加载
			if (_configs.ContainsKey(configName))
			{
				MotionLog.Error($"Config {configName} is already existed.");
				return null;
			}

			AssetConfig config;
			if (ActivatorServices != null)
				config = ActivatorServices.CreateInstance(configType) as AssetConfig;
			else
				config = Activator.CreateInstance(configType) as AssetConfig;

			if(config == null)
			{
				MotionLog.Error($"Config {configName} create instance  failed.");
			}
			else
			{
				config.Load(location);
				_configs.Add(configName, config);
			}

			return config;
		}

		/// <summary>
		/// 获取配表
		/// </summary>
		public T GetConfig<T>() where T : AssetConfig
		{
			return GetConfig(typeof(T)) as T;
		}
		public AssetConfig GetConfig(Type configType)
		{
			string configName = configType.FullName;
			foreach (var pair in _configs)
			{
				if (pair.Key== configName)
					return pair.Value;
			}

			MotionLog.Error($"Not found config {configName}");
			return null;
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

			MotionLog.Error($"Not found config {configName}");
			return null;
		}
	}
}