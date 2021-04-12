//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.Editor
{
	public class BundleBuildInfo
	{
		public string BundleLabel { private set; get; }
		public string BundleVariant { private set; get; }

		public BundleBuildInfo(string label, string variant)
		{
			BundleLabel = EditorTools.GetRegularPath(label);
			BundleVariant = variant;
		}
	}
}