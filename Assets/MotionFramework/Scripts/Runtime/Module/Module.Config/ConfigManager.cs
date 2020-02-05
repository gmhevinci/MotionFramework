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
		/// <summary>
		/// 游戏模块创建参数
		/// </summary>
		public class CreateParameters
		{
			/// <summary>
			/// 基于AssetSystem.LocationRoot的相对路径
			/// 注意：所有的配表文件必须放在该目录下
			/// </summary>
			public string BaseDirectory;
		}

		private Dictionary<string, AssetConfig> _configs = new Dictionary<string, AssetConfig>();
		private string _baseDirectory;


		void IModule.OnCreate(System.Object param)
		{
			CreateParameters createParam = param as CreateParameters;
			if (createParam == null)
				throw new ArgumentNullException($"{nameof(ConfigManager)} create param is invalid.");

			_baseDirectory = createParam.BaseDirectory;
		}
		void IModule.OnUpdate()
		{
		}
		void IModule.OnGUI()
		{
		}

		/// <summary>
		/// 加载配表
		/// </summary>
		/// <param name="configName">配表文件名称</param>
		public AssetConfig LoadConfig(string configName)
		{
			// 防止重复加载
			if (_configs.ContainsKey(configName))
			{
				MotionLog.Log(ELogLevel.Error, $"Config {configName} is already existed.");
				return null;
			}

			AssetConfig config = ConfigCreater.CreateInstance(configName);
			if (config != null)
			{
				string location = $"{_baseDirectory}/{configName}";
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
		/// <param name="configName">配表文件名称</param>
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