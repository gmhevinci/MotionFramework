//--------------------------------------------------
// Motion Framework
// Copyright©2018-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.Resource;

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
			// 检测依赖模块
			if (MotionEngine.Contains(typeof(ResourceManager)) == false)
				throw new Exception($"{nameof(ConfigManager)} depends on {nameof(ResourceManager)}");
		}
		void IModule.OnUpdate()
		{
		}
		void IModule.OnDestroy()
		{
			DestroySingleton();
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
		/// 异步加载配表
		/// </summary>
		public T LoadConfig<T>(string location) where T : AssetConfig
		{
			return LoadConfig(typeof(T), location) as T;
		}

		/// <summary>
		/// 异步加载配表
		/// </summary>
		public AssetConfig LoadConfig(Type configType, string location)
		{
			return LoadConfigInternal(configType, location, false);
		}

		/// <summary>
		/// 同步加载配表
		/// </summary>
		public T LoadConfigSync<T>(string location) where T : AssetConfig
		{
			return LoadConfigSync(typeof(T), location) as T;
		}

		/// <summary>
		/// 同步加载配表
		/// </summary>
		public AssetConfig LoadConfigSync(Type configType, string location)
		{
			return LoadConfigInternal(configType, location, true);
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

		private AssetConfig LoadConfigInternal(Type configType, string location, bool syncLoad)
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

			if (config == null)
			{
				MotionLog.Error($"Config {configName} create instance  failed.");
			}
			else
			{
				if (syncLoad)
					config.LoadSync(location);
				else
					config.Load(location);
				_configs.Add(configName, config);
			}

			return config;
		}
	}
}