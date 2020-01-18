//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Network.Coder
{
	/// <summary>
	/// 默认的网络包编码解码器
	/// </summary>
	public abstract class DefaultPackageCoder : NetworkPackageCoder
	{
		/// <summary>
		/// 包裹大小的字段类型
		/// 包裹由包头和包体组成
		/// </summary>
		public enum EPackageSizeFieldType
		{
			/// <summary>
			/// 包裹最大约64KB
			/// </summary>
			UShort = 2,

			/// <summary>
			/// 包裹最大约2GB
			/// </summary>
			Int = 4,
		}

		/// <summary>
		/// 消息ID的字段类型
		/// </summary>
		public enum EMessageIDFieldType
		{
			/// <summary>
			/// 取值范围：0 ～ 65535
			/// </summary>
			UShort = 2,

			/// <summary>
			/// 取值范围：-2147483648 ～ 2147483647 
			/// </summary>
			Int = 4,
		}


		/// <summary>
		/// 包裹大小的字段类型
		/// </summary>
		public EPackageSizeFieldType PackageSizeFieldType = EPackageSizeFieldType.UShort;

		/// <summary>
		/// 消息ID的字段类型
		/// </summary>
		public EMessageIDFieldType MessageIDFieldType = EMessageIDFieldType.UShort;


		public override void Encode(System.Object packageObj)
		{
			NetworkPackage package = (NetworkPackage)packageObj;
			if (package == null)
			{
				Channel.HandleError(false, $"The package object is invalid : {packageObj.GetType()}");
				return;
			}

			// 检测逻辑是否合法
			if(package.IsHotfixPackage)
			{
				if(package.BodyBytes == null)
				{
					Channel.HandleError(false, $"The package BodyBytes field is null : {packageObj.GetType()}");
					return;
				}
			}
			else
			{
				if(package.MsgObj == null)
				{
					Channel.HandleError(false, $"The package MsgObj field is null : {packageObj.GetType()}");
					return;
				}
			}

			// 获取包体数据
			byte[] bodyData;
			if (package.IsHotfixPackage)
				bodyData = package.BodyBytes;
			else
				bodyData = EncodeInternal(package.MsgObj);

			// 检测包体长度
			if (bodyData.Length > NetworkDefine.PackageBodyMaxSize)
			{
				Channel.HandleError(false, $"The package {package.MsgID} body size is larger than NetworkDefine.PackageBodyMaxSize");
				return;
			}

			// 写入长度
			int packetSize = (int)MessageIDFieldType + bodyData.Length;
			if (PackageSizeFieldType == EPackageSizeFieldType.UShort)
			{
				// 检测是否越界
				if (packetSize > ushort.MaxValue)
				{
					Channel.HandleError(true, $"The package {package.MsgID} size is larger than ushort.MaxValue.");
					return;
				}
				_sendBuffer.WriteUShort((ushort)packetSize);
			}
			else
			{
				_sendBuffer.WriteInt(packetSize);
			}

			// 写入包头
			{
				// 写入消息ID
				if (MessageIDFieldType == EMessageIDFieldType.UShort)
				{
					// 检测是否越界
					if (package.MsgID > ushort.MaxValue)
					{
						Channel.HandleError(true, $"The package {package.MsgID} ID is larger than ushort.MaxValue");
						return;
					}
					_sendBuffer.WriteUShort((ushort)package.MsgID);
				}
				else
				{
					_sendBuffer.WriteInt(package.MsgID);
				}
			}

			// 写入包体
			_sendBuffer.WriteBytes(bodyData, 0, bodyData.Length);
		}
		public override void Decode(List<System.Object> packageObjList)
		{
			// 循环解包
			while (true)
			{
				// 如果数据不够一个SIZE
				if (_receiveBuffer.ReadableBytes() < (int)PackageSizeFieldType)
					break;

				_receiveBuffer.MarkReaderIndex();

				// 读取Package长度
				int packageSize;
				if (PackageSizeFieldType == EPackageSizeFieldType.UShort)
					packageSize = _receiveBuffer.ReadUShort();
				else
					packageSize = _receiveBuffer.ReadInt();

				// 如果剩余可读数据小于Package长度
				if (_receiveBuffer.ReadableBytes() < packageSize)
				{
					_receiveBuffer.ResetReaderIndex();
					break; //需要退出读够数据再解包
				}

				NetworkPackage package = new NetworkPackage();

				// 读取包头
				{
					// 读取消息ID
					if (MessageIDFieldType == EMessageIDFieldType.UShort)
						package.MsgID = _receiveBuffer.ReadUShort();
					else
						package.MsgID = _receiveBuffer.ReadInt();
				}

				// 检测包体长度
				int bodySize = packageSize - (int)MessageIDFieldType;
				if (bodySize > NetworkDefine.PackageBodyMaxSize)
				{
					Channel.HandleError(true, $"The package {package.MsgID} size is larger than NetworkDefine.PackageBodyMaxSize");
					break;
				}

				// 正常解包
				try
				{			
					// 读取包体
					byte[] bodyData = _receiveBuffer.ReadBytes(bodySize);

					Type classType = NetworkMessageRegister.TryGetMessageType(package.MsgID);
					if (classType != null)
					{
						// 非热更协议
						package.MsgObj = DecodeInternal(classType, bodyData);
						if (package.MsgObj != null)
							packageObjList.Add(package);
					}
					else
					{
						// 热更协议
						package.IsHotfixPackage = true;
						package.BodyBytes = bodyData;
						packageObjList.Add(package);
					}
				}
				catch (Exception ex)
				{
					// 解包异常后继续解包
					Channel.HandleError(false, $"The package {package.MsgID} decode error : {ex.ToString()}");
				}
			} //while end

			// 注意：将剩余数据移至起始
			_receiveBuffer.DiscardReadBytes();
		}

		/// <summary>
		/// 消息内部编码
		/// </summary>
		/// <param name="msgObj">消息对象</param>
		/// <returns>返回序列化后的包体数据</returns>
		protected abstract byte[] EncodeInternal(System.Object msgObj);

		/// <summary>
		/// 消息内部解码
		/// </summary>
		/// <param name="classType">消息类的类型</param>
		/// <param name="bodyBytes">包体数据</param>
		/// <returns>返回反序列化后的消息对象</returns>
		protected abstract System.Object DecodeInternal(System.Type classType, byte[] bodyBytes);
	}
}