//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace MotionFramework.Patch
{
	/// <summary>
	/// 变体规则
	/// </summary>
	public class VariantRule
	{
		public const string DefaultTag = "default";

		/// <summary>
		/// 变体组
		/// </summary>
		public List<string> VariantGroup;

		/// <summary>
		/// 目标变体
		/// </summary>
		public string TargetVariant;
	}
}