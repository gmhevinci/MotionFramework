//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	public static class TextureHelper
	{
		/// <summary>
		/// 获取纹理运行时内存大小
		/// </summary>
		public static int GetStorageMemorySize(Texture tex)
		{
			var assembly = typeof(AssetDatabase).Assembly;
			var type = assembly.GetType("UnityEditor.TextureUtil");
			int size = (int)EditorTools.InvokePublicStaticMethod(type, "GetStorageMemorySize", tex);
			return size;
		}

		/// <summary>
		/// 获取当前平台纹理的格式
		/// </summary>
		public static TextureFormat GetTextureFormatString(Texture tex)
		{
			var assembly = typeof(AssetDatabase).Assembly;
			var type = assembly.GetType("UnityEditor.TextureUtil");
			TextureFormat format = (TextureFormat)EditorTools.InvokePublicStaticMethod(type, "GetTextureFormat", tex);
			return format;
		}
	}
}