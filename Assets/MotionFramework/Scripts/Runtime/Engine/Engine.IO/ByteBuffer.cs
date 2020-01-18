//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

#if MOTION_SERVER
	using System.Numerics;
#else
	using UnityEngine;
#endif

namespace MotionFramework.IO
{
	/// <summary>
	/// 字节缓冲区
	/// </summary>
	public class ByteBuffer
	{
		public byte[] Buf { get; }
		public int ReaderIndex { set; get; }
		public int WriterIndex { set; get; }
		public int Capacity
		{
			get { return Buf.Length; }
		}

		private int _markedReaderIndex;
		private int _markedWriterIndex;


		public ByteBuffer(int capacity)
		{
			Buf = new byte[capacity];
		}
		public ByteBuffer(byte[] buffer)
		{
			Buf = buffer;
			WriterIndex = buffer.Length;
		}

		/// <summary>
		/// 清空缓冲区
		/// </summary>
		public void Clear()
		{
			ReaderIndex = 0;
			WriterIndex = 0;
		}

		/// <summary>
		/// 移动未读数据到最前列
		/// </summary>
		public void DiscardReadBytes()
		{
			if (ReaderIndex == 0)
			{
				return;
			}

			if (ReaderIndex == WriterIndex)
			{
				ReaderIndex = 0;
				WriterIndex = 0;
			}
			else
			{
				for (int i = 0, j = ReaderIndex, len = WriterIndex - ReaderIndex; i < len; i++, j++)
				{
					Buf[i] = Buf[j];
				}
				WriterIndex -= ReaderIndex;
				ReaderIndex = 0;
			}
		}

		public int ReadableBytes()
		{
			return WriterIndex - ReaderIndex;
		}
		public bool IsReadable(int size = 1)
		{
			return WriterIndex - ReaderIndex >= size;
		}
		public void MarkReaderIndex()
		{
			_markedReaderIndex = ReaderIndex;
		}
		public void ResetReaderIndex()
		{
			ReaderIndex = _markedReaderIndex;
		}

		public int WriteableBytes()
		{
			return Capacity - WriterIndex;
		}
		public bool IsWriteable(int size = 1)
		{
			return Capacity - WriterIndex >= size;
		}
		public void MarkWriterIndex()
		{
			_markedWriterIndex = WriterIndex;
		}
		public void ResetWriterIndex()
		{
			WriterIndex = _markedWriterIndex;
		}

		#region 读取操作
		[Conditional("DEBUG")]
		private void CheckReaderIndex(int len)
		{
			if (ReaderIndex + len > WriterIndex)
			{
				throw new IndexOutOfRangeException();
			}
		}

		public bool ReadBool()
		{
			CheckReaderIndex(1);
			return (Buf[ReaderIndex++] == 1);
		}
		public byte ReadByte()
		{
			CheckReaderIndex(1);
			return Buf[ReaderIndex++];
		}
		public sbyte ReadSbyte()
		{
			return (sbyte)ReadByte();
		}
		public byte[] ReadBytes(int count)
		{
			CheckReaderIndex(count);
			var dst = new byte[count];
			Buffer.BlockCopy(Buf, ReaderIndex, dst, 0, count);
			ReaderIndex += count;
			return dst;
		}
		public double ReadDouble()
		{
			CheckReaderIndex(8);
			ReverseOrder(Buf, ReaderIndex, 8);
			double result = BitConverter.ToDouble(Buf, ReaderIndex);
			ReaderIndex += 8;
			return result;
		}
		public float ReadFloat()
		{
			CheckReaderIndex(4);
			ReverseOrder(Buf, ReaderIndex, 4);
			float result = BitConverter.ToSingle(Buf, ReaderIndex);
			ReaderIndex += 4;
			return result;
		}
		public int ReadInt()
		{
			CheckReaderIndex(4);
			ReverseOrder(Buf, ReaderIndex, 4);
			int result = BitConverter.ToInt32(Buf, ReaderIndex);
			ReaderIndex += 4;
			return result;
		}
		public uint ReadUInt()
		{
			CheckReaderIndex(4);
			ReverseOrder(Buf, ReaderIndex, 4);
			uint result = BitConverter.ToUInt32(Buf, ReaderIndex);
			ReaderIndex += 4;
			return result;
		}
		public long ReadLong()
		{
			CheckReaderIndex(8);
			ReverseOrder(Buf, ReaderIndex, 8);
			long result = BitConverter.ToInt64(Buf, ReaderIndex);
			ReaderIndex += 8;
			return result;
		}
		public short ReadShort()
		{
			CheckReaderIndex(2);
			ReverseOrder(Buf, ReaderIndex, 2);
			short result = BitConverter.ToInt16(Buf, ReaderIndex);
			ReaderIndex += 2;
			return result;
		}
		public ushort ReadUShort()
		{
			CheckReaderIndex(2);
			ReverseOrder(Buf, ReaderIndex, 2);
			ushort result = BitConverter.ToUInt16(Buf, ReaderIndex);
			ReaderIndex += 2;
			return result;
		}
		public string ReadUTF()
		{
			CheckReaderIndex(2);
			ushort count = ReadUShort();
			CheckReaderIndex(count);
			string str = Encoding.UTF8.GetString(Buf, ReaderIndex, count - 1);
			ReaderIndex += count;
			return str;
		}
		public string ReadUTF(int count)
		{
			CheckReaderIndex(count);
			string str = Encoding.UTF8.GetString(Buf, ReaderIndex, count);
			ReaderIndex += count;
			return str.Split('\0')[0]; // 注意：服务器发回来的字符串最后结尾很有可能带数个\0
		}
		public List<string> ReadListUTF()
		{
			List<string> list = new List<string>();
			int count = ReadInt();

			for (int i = 0; i < count; i++)
				list.Add(ReadUTF());

			return list;
		}
		public List<float> ReadListFloat()
		{
			List<float> list = new List<float>();
			int count = ReadInt();

			for (int i = 0; i < count; i++)
				list.Add(ReadFloat());

			return list;
		}
		public List<double> ReadListDouble()
		{
			List<double> list = new List<double>();
			int count = ReadInt();

			for (int i = 0; i < count; i++)
				list.Add(ReadDouble());

			return list;
		}
		public List<int> ReadListInt()
		{
			List<int> list = new List<int>();
			int count = ReadInt();

			for (int i = 0; i < count; i++)
				list.Add(ReadInt());

			return list;
		}
		public List<long> ReadListLong()
		{
			List<long> list = new List<long>();
			int count = ReadInt();

			for (int i = 0; i < count; i++)
				list.Add(ReadLong());

			return list;
		}
		public Vector3 ReadVector3()
		{
			float x = ReadFloat();
			float y = ReadFloat();
			float z = ReadFloat();
			return new Vector3(x, y, z);
		}
		public Vector2 ReadVector2()
		{
			float x = ReadFloat();
			float y = ReadFloat();
			return new Vector2(x, y);
		}
		#endregion

		#region 写入操作
		[Conditional("DEBUG")]
		private void CheckWriterIndex(int len)
		{
			if (WriterIndex + len > Capacity)
			{
				throw new IndexOutOfRangeException();
			}
		}

		public void WriteBool(bool b)
		{
			WriteByte((byte)(b ? 1 : 0));
		}
		public void WriteByte(byte b)
		{
			CheckWriterIndex(1);
			Buf[WriterIndex++] = b;
		}
		public void WriteSbyte(sbyte b)
		{
			// 注意：从sbyte强转到byte不会有数据变化或丢失
			WriteByte((byte)b);
		}
		public void WriteBytes(byte[] data, int ofs = -1, int count = -1)
		{
			if (ofs == -1 || count == -1)
			{
				ofs = 0;
				count = data.Length;
			}
			CheckWriterIndex(count);
			Buffer.BlockCopy(data, ofs, Buf, WriterIndex, count);
			WriterIndex += count;
		}
		public void WriteDouble(double d)
		{
			byte[] bytes = BitConverter.GetBytes(d);
			ReverseOrder(bytes);
			WriteBytes(bytes);
		}
		public void WriteFloat(float f)
		{
			byte[] bytes = BitConverter.GetBytes(f);
			ReverseOrder(bytes);
			WriteBytes(bytes);
		}
		public void WriteInt(int i)
		{
			byte[] bytes = BitConverter.GetBytes(i);
			ReverseOrder(bytes);
			WriteBytes(bytes);
		}
		public void WriteUInt(uint i)
		{
			byte[] bytes = BitConverter.GetBytes(i);
			ReverseOrder(bytes);
			WriteBytes(bytes);
		}
		public void WriteLong(long l)
		{
			byte[] bytes = BitConverter.GetBytes(l);
			ReverseOrder(bytes);
			WriteBytes(bytes);
		}
		public void WriteShort(short s)
		{
			byte[] bytes = BitConverter.GetBytes(s);
			ReverseOrder(bytes);
			WriteBytes(bytes);
		}
		public void WriteUShort(ushort us)
		{
			byte[] bytes = BitConverter.GetBytes(us);
			ReverseOrder(bytes);
			WriteBytes(bytes);
		}
		public void WriteUTF(string str)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			int num = bytes.Length;
			if (num > 0x8000)
			{
				throw new FormatException("String length cannot be greater then 32768 !");
			}
			WriteUShort(Convert.ToUInt16(num + 1));
			WriteBytes(bytes);
			WriteByte(((byte)'\0'));
		}
		public void WriteUTF(string str, int count)
		{
			if (count > 0x8000)
			{
				throw new FormatException("String length cannot be greater then 32768 !");
			}

			// 如果传入的字符串为空
			if (string.IsNullOrEmpty(str))
			{
				byte[] bytes = new byte[count];
				for (int j = 0; j < count; j++)
				{
					bytes[j] = 0;
				}
				WriteBytes(bytes);
				return;
			}

			// 当传入字符串大于限制最大字符串时进行截断处理
			{
				byte[] bytes = new byte[count];
				byte[] availablebytes = Encoding.UTF8.GetBytes(str);
				int num = availablebytes.Length;
				if (count <= num)
				{
					Buffer.BlockCopy(availablebytes, 0, bytes, 0, count);
					bytes[count - 1] = 0;
				}
				else
				{
					Buffer.BlockCopy(availablebytes, 0, bytes, 0, num);
					for (int j = num; j < count; j++)
					{
						bytes[j] = 0;
					}
				}
				WriteBytes(bytes);
			}
		}
		public void WriteListUTF(List<string> list)
		{
			int count = 0;
			if (list != null)
				count = list.Count;

			WriteInt(count);

			for (int i = 0; i < count; i++)
				WriteUTF(list[i]);
		}
		public void WriteListFloat(List<float> list)
		{
			int count = 0;
			if (list != null)
				count = list.Count;

			WriteInt(count);

			for (int i = 0; i < count; i++)
				WriteFloat(list[i]);
		}
		public void WriteListDouble(List<double> list)
		{
			int count = 0;
			if (list != null)
				count = list.Count;

			WriteInt(count);

			for (int i = 0; i < count; i++)
				WriteDouble(list[i]);
		}
		public void WriteListInt(List<int> list)
		{
			int count = 0;
			if (list != null)
				count = list.Count;

			WriteInt(count);

			for (int i = 0; i < count; i++)
				WriteInt(list[i]);
		}
		public void WriteListLong(List<long> list)
		{
			int count = 0;
			if (list != null)
				count = list.Count;

			WriteInt(count);

			for (int i = 0; i < count; i++)
				WriteLong(list[i]);
		}

#if MOTION_SERVER
		public void WriteVector3(Vector3 v)
		{
			WriteFloat(v.X);
			WriteFloat(v.Y);
			WriteFloat(v.Z);
		}
		public void WriteVector2(Vector2 v)
		{
			WriteFloat(v.X);
			WriteFloat(v.Y);
		}
#else
		public void WriteVector3(Vector3 v)
		{
			WriteFloat(v.x);
			WriteFloat(v.y);
			WriteFloat(v.z);
		}
		public void WriteVector2(Vector2 v)
		{
			WriteFloat(v.x);
			WriteFloat(v.y);
		}
#endif
		#endregion

		[Conditional("LITTLE_ENDIAN")]
		private void ReverseOrder(byte[] dt, int startIndex = -1, int len = -1)
		{
			if (startIndex == -1 || len == -1)
			{
				startIndex = 0;
				len = dt.Length;
			}

			if (len <= 1)
				return;

			int num = startIndex + len - 1;
			byte tb = 0;
			for (int i = startIndex, max = startIndex - 1 + (len >> 1); i <= max; i++, num--)
			{
				tb = dt[num];
				dt[num] = dt[i];
				dt[i] = tb;
			}
		}
	}
}