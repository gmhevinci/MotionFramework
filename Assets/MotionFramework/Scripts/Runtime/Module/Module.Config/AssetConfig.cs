//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.IO;
using MotionFramework.Resource;

namespace MotionFramework.Config
{
	/// <summary>
	/// 配表数据类
	/// </summary>
	public abstract class ConfigTable
	{
		public int Id { get; protected set; }
		public abstract void ReadByte(ByteBuffer byteBuf);
	}

	/// <summary>
	/// 配表资源类
	/// </summary>
	public abstract class AssetConfig
	{
		private AssetReference _assetRef;
		private AssetOperationHandle _handle;
		private System.Action<AssetConfig> _userCallback;

		/// <summary>
		/// 配表数据集合
		/// </summary>
		protected readonly Dictionary<int, ConfigTable> _tables = new Dictionary<int, ConfigTable>();

		/// <summary>
		/// 是否完成
		/// </summary>
		public bool IsDone
		{
			get
			{
				return _handle.IsDone;
			}
		}

		/// <summary>
		/// 资源地址
		/// </summary>
		public string Location { private set; get; }


		public void Init(string location)
		{
			if (_assetRef != null)
				return;

			Location = location;
			_assetRef = new AssetReference(location);
		}
		public void Load(System.Action<AssetConfig> callback)
		{
			if (_userCallback != null)
				return;

			_userCallback = callback;
			_handle = _assetRef.LoadAssetAsync<TextAsset>();
			_handle.Completed += Handle_Completed;
		}
		private void Handle_Completed(AssetOperationHandle obj)
		{
			try
			{
				TextAsset txt = _handle.AssetObject as TextAsset;
				if (txt != null)
				{
					// 解析数据
					ParseDataInternal(txt.bytes);
				}
			}
			catch (Exception ex)
			{
				MotionLog.Log(ELogLevel.Error, $"Failed to parse config {Location}. Error : {ex.ToString()}");
			}

			// 注意：为了节省内存这里立即释放了资源
			if (_assetRef != null)
			{
				_assetRef.Release();
				_assetRef = null;
			}

			_userCallback?.Invoke(this);
		}

		/// <summary>
		/// 序列化表格的接口
		/// </summary>
		protected abstract ConfigTable ReadTable(ByteBuffer byteBuffer);

		/// <summary>
		/// 解析数据
		/// </summary>
		private void ParseDataInternal(byte[] bytes)
		{
			ByteBuffer bb = new ByteBuffer(bytes);

			int tabLine = 1;
			const int headMarkAndSize = 6;
			while (bb.IsReadable(headMarkAndSize))
			{
				// 检测行标记
				short tabHead = bb.ReadShort();
				if (tabHead != ConfigDefine.TabStreamHead)
				{
					throw new Exception($"Table stream head is invalid. File is {Location} , tab line is {tabLine}");
				}

				// 检测行大小
				int tabSize = bb.ReadInt();
				if (!bb.IsReadable(tabSize) || tabSize > ConfigDefine.TabStreamMaxLen)
				{
					throw new Exception($"Table stream size is invalid. File is {Location}, tab line {tabLine}");
				}

				// 读取行内容
				ConfigTable tab = null;
				try
				{
					tab = ReadTable(bb);
				}
				catch (Exception ex)
				{
					throw new Exception($"ReadTab falied. File is {Location}, tab line {tabLine}. Error : {ex.ToString()}");
				}

				++tabLine;

				// 检测是否重复
				if (_tables.ContainsKey(tab.Id))
				{
					throw new Exception($"The tab key is already exist. Type is {this.GetType()}, file is {Location}, key is { tab.Id}");
				}
				else
				{
					_tables.Add(tab.Id, tab);
				}
			}
		}

		/// <summary>
		/// 通过外部传进的数据来组织表
		/// </summary>
		public void ParseDataFromCustomData(byte[] bytes)
		{
			_tables.Clear();
			ParseDataInternal(bytes);
		}


		/// <summary>
		/// 获取数据，如果不存在报警告
		/// </summary>
		public ConfigTable GetTable(int key)
		{
			if (_tables.ContainsKey(key))
			{
				return _tables[key];
			}
			else
			{
				MotionLog.Log(ELogLevel.Warning, $"Faild to get tab. File is {Location}, key is {key}");
				return null;
			}
		}

		/// <summary>
		/// 获取数据，如果不存在不会报警告
		/// </summary>
		public bool TryGetTable(int key, out ConfigTable table)
		{
			return _tables.TryGetValue(key, out table);
		}

		/// <summary>
		/// 是否包含Key
		/// </summary>
		public bool ContainsKey(int key)
		{
			return _tables.ContainsKey(key);
		}

		/// <summary>
		/// 获取所有Key
		/// </summary>
		public List<int> GetKeys()
		{
			List<int> keys = new List<int>();
			foreach (var tab in _tables)
			{
				keys.Add(tab.Key);
			}
			return keys;
		}
	}
}