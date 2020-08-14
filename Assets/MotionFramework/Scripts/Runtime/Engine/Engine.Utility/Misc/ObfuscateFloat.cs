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
	public struct ObfuscateFloat
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
		public float Value
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

		public static implicit operator float(ObfuscateFloat value) { return value.Value; }
		public static implicit operator ObfuscateFloat(float value) { return new ObfuscateFloat(value); }
	}
}