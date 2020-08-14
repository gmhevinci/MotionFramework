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
    public struct ObfuscateLong
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
        public long Value
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

        public static implicit operator long(ObfuscateLong value) { return value.Value; }
        public static implicit operator ObfuscateLong(long value) { return new ObfuscateLong(value); }
    }
}