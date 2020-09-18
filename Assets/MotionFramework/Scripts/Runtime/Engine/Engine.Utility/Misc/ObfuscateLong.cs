//--------------------------------------------------
// Motion Framework
// Copyright©2020-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using UnityEngine;

namespace MotionFramework.Utility
{
	[Serializable]
	public struct ObfuscateLong : IFormattable, IEquatable<ObfuscateLong>, IComparable<ObfuscateLong>, IComparable<long>, IComparable
	{
		private static long GlobalSeed = DateTime.Now.Ticks;

		[SerializeField]
		private long _seed;
		[SerializeField]
		private long _data;

		public ObfuscateLong(long value)
		{
			_seed = GlobalSeed++;
			_data = 0;
			Value = value;
		}
		internal long Value
		{
			get
			{
				var v = _data ^ _seed;
				return v;
			}
			set
			{
				_data = value ^ _seed;
			}
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
		public override string ToString()
		{
			return Value.ToString();
		}
		public override bool Equals(object obj)
		{
			return obj is ObfuscateLong && Equals((ObfuscateLong)obj);
		}

		public string ToString(string format)
		{
			return Value.ToString(format);
		}
		public string ToString(IFormatProvider provider)
		{
			return Value.ToString(provider);
		}
		public string ToString(string format, IFormatProvider provider)
		{
			return Value.ToString(format, provider);
		}

		public bool Equals(ObfuscateLong obj)
		{
			return Value.Equals(obj.Value);
		}
		public int CompareTo(ObfuscateLong other)
		{
			return Value.CompareTo(other.Value);
		}
		public int CompareTo(long other)
		{
			return Value.CompareTo(other);
		}
		public int CompareTo(object obj)
		{
			return Value.CompareTo(obj);
		}

		#region 运算符重载
		public static implicit operator long(ObfuscateLong value)
		{
			return value.Value;
		}
		public static implicit operator ObfuscateLong(long value)
		{
			return new ObfuscateLong(value);
		}
		public static ObfuscateLong operator ++(ObfuscateLong value)
		{
			return value.Value + 1;
		}
		public static ObfuscateLong operator --(ObfuscateLong value)
		{
			return value.Value - 1;
		}
		#endregion
	}
}