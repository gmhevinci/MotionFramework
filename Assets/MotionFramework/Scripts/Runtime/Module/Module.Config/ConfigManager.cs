//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MotionFramework.Utility;

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
			/// 配表类所属的程序集名称
			/// 默认的程序集名称为：Assembly-CSharp
			/// </summary>
			public string ConfigAssemblyName = AssemblyUtility.UnityDefaultAssemblyName;
		}

		private readonly Dictionary<string, AssetConfig> _configs = new Dictionary<string, AssetConfig>();
		private readonly ConfigCreater _creater = new ConfigCreater();

		void IModule.OnCreate(System.Object param)
		{
			CreateParameters createParam = param as CreateParameters;
			if (createParam == null)
				throw new Exception($"{nameof(ConfigManager)} create param is invalid.");

			_creater.Initialize(createParam.ConfigAssemblyName);
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
				MotionLog.Error($"Config {configName} is already existed.");
				return null;
			}

			AssetConfig config = _creater.CreateInstance(configName);
			if (config != null)
			{
				config.Load(location);
				_configs.Add(configName, config);
			}
			else
			{
				MotionLog.Error($"Config type {configName} is invalid.");
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

			MotionLog.Error($"Not found config {configName}");
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

			MotionLog.Error($"Not found config {type}");
			return null;
		}
	}
}