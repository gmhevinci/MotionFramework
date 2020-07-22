//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace UnityEngine
{
	public static partial class UnityEngine_Object_Extension
	{
		public static bool IsDestroyed(this UnityEngine.Object o)
		{
			return o == null;
		}
	}
}