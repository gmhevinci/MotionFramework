//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Network
{
	/// <summary>
	/// 非热更新网络消息类型注册器
	/// </summary>
	public class NetworkMessageRegister
	{
		private static Dictionary<int, Type> _types = new Dictionary<int, Type>();
		
		/// <summary>
		/// 注册非热更的消息类型
		/// </summary>
		public static void RegisterMessageType(int msgID, Type classType)
		{
			// 判断是否重复
			if (_types.ContainsKey(msgID))
				throw new Exception($"NetMessage {msgID} already exist.");

			_types.Add(msgID, classType);
		}

		/// <summary>
		/// 获取指定消息ID的消息类型
		/// </summary>
		/// <param name="msgID">消息ID</param>
		/// <returns>如果找不到会报异常</returns>
		public static Type GetMessageType(int msgID)
		{
			Type type;
			if (_types.TryGetValue(msgID, out type))
			{
				return type;
			}
			else
			{
				throw new KeyNotFoundException($"NetMessage {msgID} is not define.");
			}
		}

		/// <summary>
		/// 尝试获取指定消息ID的消息类型
		/// </summary>
		/// <param name="msgID">消息ID</param>
		/// <returns>如果找不到会返回NULL</returns>
		public static Type TryGetMessageType(int msgID)
		{
			Type type;
			_types.TryGetValue(msgID, out type);
			return type;
		}
	}
}