//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Editor
{
	public interface IAssetScaner
	{
		List<ScanReport> Scan(string directory);
	}
}