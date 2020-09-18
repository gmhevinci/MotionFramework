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
	public struct ObfuscateInt : IFormattable, IEquatable<ObfuscateInt>, IComparable<ObfuscateInt>, IComparable<int>, IComparable
	{
		private static int GlobalSeed = (int)DateTime.Now.Ticks;

		[SerializeField]
		private int _seed;
		[SerializeField]
		private int _data;

		public ObfuscateInt(int value)
		{
			_seed = GlobalSeed++;
			_data = 0;
			Value = value;
		}
		internal int Value
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
			return obj is ObfuscateInt && Equals((ObfuscateInt)obj);
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

		public bool Equals(ObfuscateInt obj)
		{
			return Value.Equals(obj.Value);
		}
		public int CompareTo(ObfuscateInt other)
		{
			return Value.CompareTo(other.Value);
		}
		public int CompareTo(int other)
		{
			return Value.CompareTo(other);
		}
		public int CompareTo(object obj)
		{
			return Value.CompareTo(obj);
		}

		#region 运算符重载
		public static implicit operator int(ObfuscateInt value)
		{
			return value.Value;
		}
		public static implicit operator ObfuscateInt(int value)
		{
			return new ObfuscateInt(value);
		}
		public static implicit operator ObfuscateFloat(ObfuscateInt value)
		{
			return value.Value;
		}
		public static implicit operator ObfuscateDouble(ObfuscateInt value)
		{
			return value.Value;
		}
		public static ObfuscateInt operator ++(ObfuscateInt value)
		{
			return value.Value + 1;
		}
		public static ObfuscateInt operator --(ObfuscateInt value)
		{
			return value.Value - 1;
		}
		#endregion
	}
}