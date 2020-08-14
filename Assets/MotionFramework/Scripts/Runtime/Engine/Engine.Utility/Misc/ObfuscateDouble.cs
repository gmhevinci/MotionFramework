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
	public struct ObfuscateDouble
	{
		private static long GlobalSeed = DateTime.Now.Ticks;

		[SerializeField]
		private long _seed;
		[SerializeField]
		private long _data;

		public ObfuscateDouble(double value)
		{
			_seed = GlobalSeed++;
			_data = 0;
			Value = value;
		}
		public double Value
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

		public override string ToString()
		{
			return Value.ToString();
		}
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		unsafe static long ConvertValue(double value)
		{
			double* ptr = &value;
			return *((long*)ptr);
		}
		unsafe static double ConvertValue(long value)
		{
			long* ptr = &value;
			return *((double*)ptr);
		}

		public static implicit operator double(ObfuscateDouble value) { return value.Value; }
		public static implicit operator ObfuscateDouble(double value) { return new ObfuscateDouble(value); }
	}
}