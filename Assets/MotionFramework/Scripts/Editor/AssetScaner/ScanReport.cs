//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Editor
{
	public class ScanReport
	{
		public string AssetPath { private set; get; }
		public string ReportInfo { private set; get; }
		public UnityEngine.Object AssetObject { private set; get; }

		public ScanReport(string assetPath, string reportInfo, UnityEngine.Object assetObject)
		{
			AssetPath = assetPath;
			ReportInfo = reportInfo;
			AssetObject = assetObject;
		}
	}
}