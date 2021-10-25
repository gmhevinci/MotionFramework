//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using MotionFramework.IO;

namespace MotionFramework.Network
{
	/// <summary>
	/// 默认的网络包编码解码器
	/// </summary>
	public abstract class DefaultNetworkPackageCoder : NetworkPackageCoder
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


		/// <summary>
		/// 获取包头的尺寸
		/// </summary>
		public override int GetPackageHeaderSize()
		{
			int size = 0;
			size += (int)PackageSizeFieldType;
			size += (int)MessageIDFieldType;
			return size;
		}

		/// <summary>
		/// 编码
		/// </summary>
		public override void Encode(ByteBuffer sendBuffer, System.Object packageObj)
		{
			DefaultNetworkPackage package = (DefaultNetworkPackage)packageObj;
			if (package == null)
			{
				HandleError(false, $"The package object is invalid : {packageObj.GetType()}");
				return;
			}

			// 检测逻辑是否合法
			if (package.IsHotPackage)
			{
				if (package.BodyBytes == null)
				{
					HandleError(false, $"The package BodyBytes field is null : {packageObj.GetType()}");
					return;
				}
			}
			else
			{
				if (package.MsgObj == null)
				{
					HandleError(false, $"The package MsgObj field is null : {packageObj.GetType()}");
					return;
				}
			}

			// 获取包体数据
			byte[] bodyData;
			if (package.IsHotPackage)
				bodyData = package.BodyBytes;
			else
				bodyData = EncodeInternal(package.MsgObj);

			// 检测包体长度
			if (bodyData.Length > PackageBodyMaxSize)
			{
				HandleError(false, $"The package {package.MsgID} body size is larger than NetworkDefine.PackageBodyMaxSize");
				return;
			}

			// 写入长度
			int packetSize = (int)MessageIDFieldType + bodyData.Length;
			if (PackageSizeFieldType == EPackageSizeFieldType.UShort)
			{
				// 检测是否越界
				if (packetSize > ushort.MaxValue)
				{
					HandleError(true, $"The package {package.MsgID} size is larger than ushort.MaxValue.");
					return;
				}
				sendBuffer.WriteUShort((ushort)packetSize);
			}
			else
			{
				sendBuffer.WriteInt(packetSize);
			}

			// 写入包头
			{
				// 写入消息ID
				if (MessageIDFieldType == EMessageIDFieldType.UShort)
				{
					// 检测是否越界
					if (package.MsgID > ushort.MaxValue)
					{
						HandleError(true, $"The package {package.MsgID} ID is larger than ushort.MaxValue");
						return;
					}
					sendBuffer.WriteUShort((ushort)package.MsgID);
				}
				else
				{
					sendBuffer.WriteInt(package.MsgID);
				}
			}

			// 写入包体
			sendBuffer.WriteBytes(bodyData, 0, bodyData.Length);
		}

		/// <summary>
		/// 解码
		/// </summary>
		public override void Decode(ByteBuffer receiveBuffer, List<System.Object> outputResult)
		{
			// 循环解包
			while (true)
			{
				// 如果数据不够一个SIZE
				if (receiveBuffer.ReadableBytes < (int)PackageSizeFieldType)
					break;

				receiveBuffer.MarkReaderIndex();

				// 读取Package长度
				int packageSize;
				if (PackageSizeFieldType == EPackageSizeFieldType.UShort)
					packageSize = receiveBuffer.ReadUShort();
				else
					packageSize = receiveBuffer.ReadInt();

				// 如果剩余可读数据小于Package长度
				if (receiveBuffer.ReadableBytes < packageSize)
				{
					receiveBuffer.ResetReaderIndex();
					break; //需要退出读够数据再解包
				}

				DefaultNetworkPackage package = new DefaultNetworkPackage();

				// 读取包头
				{
					// 读取消息ID
					if (MessageIDFieldType == EMessageIDFieldType.UShort)
						package.MsgID = receiveBuffer.ReadUShort();
					else
						package.MsgID = receiveBuffer.ReadInt();
				}

				// 检测包体长度
				int bodySize = packageSize - (int)MessageIDFieldType;
				if (bodySize > PackageBodyMaxSize)
				{
					HandleError(true, $"The package {package.MsgID} size is larger than {PackageBodyMaxSize} !");
					break;
				}

				// 正常解包
				try
				{
					// 读取包体
					byte[] bodyData = receiveBuffer.ReadBytes(bodySize);

					Type classType = NetworkMessageRegister.TryGetMessageType(package.MsgID);
					if (classType != null)
					{
						// 非热更协议
						package.MsgObj = DecodeInternal(classType, bodyData);
						if (package.MsgObj != null)
							outputResult.Add(package);
					}
					else
					{
						// 热更协议
						package.IsHotPackage = true;
						package.BodyBytes = bodyData;
						outputResult.Add(package);
					}
				}
				catch (Exception ex)
				{
					// 解包异常后继续解包
					HandleError(false, $"The package {package.MsgID} decode error : {ex.ToString()}");
				}
			} //while end

			// 注意：将剩余数据移至起始
			receiveBuffer.DiscardReadBytes();
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