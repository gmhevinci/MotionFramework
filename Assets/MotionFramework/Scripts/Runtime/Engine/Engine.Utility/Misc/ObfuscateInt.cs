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
	public struct ObfuscateInt
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
		public int Value
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

		public override string ToString()
		{
			return Value.ToString();
		}
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public static implicit operator int(ObfuscateInt value) { return value.Value; }
		public static implicit operator ObfuscateInt(int value) { return new ObfuscateInt(value); }
	}
}