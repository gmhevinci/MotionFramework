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
	public struct ObfuscateFloat : IFormattable, IEquatable<ObfuscateFloat>, IComparable<ObfuscateFloat>, IComparable<float>, IComparable
	{
		private static int GlobalSeed = (int)DateTime.Now.Ticks;

		[SerializeField]
		private int _seed;
		[SerializeField]
		private int _data;

		public ObfuscateFloat(float value)
		{
			_seed = GlobalSeed++;
			_data = 0;
			Value = value;
		}
		internal float Value
		{
			get
			{
				var v = _data ^ _seed;
				return ConvertValue(v);
			}
			set
			{
				var v = ConvertValue(value);
				_data = v ^ _seed;
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
			return obj is ObfuscateFloat && Equals((ObfuscateFloat)obj);
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

		public bool Equals(ObfuscateFloat obj)
		{
			return obj.Value.Equals(Value);
		}
		public int CompareTo(ObfuscateFloat other)
		{
			return Value.CompareTo(other.Value);
		}
		public int CompareTo(float other)
		{
			return Value.CompareTo(other);
		}
		public int CompareTo(object obj)
		{
			return Value.CompareTo(obj);
		}

		#region 运算符重载
		public static implicit operator float(ObfuscateFloat value)
		{
			return value.Value;
		}
		public static implicit operator ObfuscateFloat(float value)
		{
			return new ObfuscateFloat(value);
		}
		#endregion

		unsafe static int ConvertValue(float value)
		{
			float* ptr = &value;
			return *((int*)ptr);
		}
		unsafe static float ConvertValue(int value)
		{
			int* ptr = &value;
			return *((float*)ptr);
		}
	}
}