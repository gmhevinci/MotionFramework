//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections.Generic;

namespace MotionFramework.Editor
{
	public class DefaultScaner : IAssetScaner
	{
		List<ScanReport> IAssetScaner.Scan(string directory)
		{
			throw new System.NotImplementedException();
		}
	}
}