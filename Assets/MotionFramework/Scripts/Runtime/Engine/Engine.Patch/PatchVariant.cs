//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MotionFramework.Patch
{
	[Serializable]
	public class PatchVariant
	{
		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName;

		/// <summary>
		/// 变体类型列表
		/// </summary>
		public List<string> Variants;

		public PatchVariant(string bundleName, List<string> variants)
		{
			BundleName = bundleName;
			Variants = variants;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder(100);
			builder.Append(BundleName);
			builder.Append(" = ");
			if (Variants != null)
			{
				for (int i = 0; i < Variants.Count; i++)
				{
					builder.Append(Variants[i]);
					if (i < Variants.Count - 1)
						builder.Append("|");
				}
			}
			return builder.ToString();
		}
	}
}