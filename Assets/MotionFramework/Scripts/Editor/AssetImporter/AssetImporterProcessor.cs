//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.IO;
using System;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 资源导入管理类
	/// </summary>
	public class AssetImporterProcessor : AssetPostprocessor
	{
		#region 模型处理
		public void OnPreprocessModel()
		{
			if (AssetImporterSettingData.Setting.Toggle == false)
				return;

			string importAssetPath = this.assetPath;
			IAssetProcessor processor = AssetImporterSettingData.GetCustomProcessor(importAssetPath);
			if (processor != null)
				processor.OnPreprocessModel(importAssetPath, this.assetImporter);
		}
		public void OnPostprocessModel(GameObject go)
		{
		}
		#endregion

		#region 纹理处理
		public void OnPreprocessTexture()
		{
			if (AssetImporterSettingData.Setting.Toggle == false)
				return;

			string importAssetPath = this.assetPath;
			IAssetProcessor processor = AssetImporterSettingData.GetCustomProcessor(importAssetPath);
			if (processor != null)
				processor.OnPreprocessTexture(importAssetPath, this.assetImporter);
		}
		public void OnPostprocessTexture(Texture2D texture)
		{
		}
		public void OnPostprocessSprites(Texture2D texture, Sprite[] sprites)
		{
		}
		#endregion

		#region 音频处理
		public void OnPreprocessAudio()
		{
			if (AssetImporterSettingData.Setting.Toggle == false)
				return;

			string importAssetPath = this.assetPath;
			IAssetProcessor processor = AssetImporterSettingData.GetCustomProcessor(importAssetPath);
			if (processor != null)
				processor.OnPreprocessAudio(importAssetPath, this.assetImporter);
		}
		public void OnPostprocessAudio(AudioClip clip)
		{
		}
		#endregion

		#region 其他处理
		/// <summary>
		/// 注意：该方法从Unity2018版本开始才起效
		/// </summary>
		public void OnPreprocessAsset()
		{
			string extension = Path.GetExtension(this.assetPath);
			if (extension == ".spriteatlas")
			{
				OnPreprocessSpriteAtlas();
			}
		}

		/// <summary>
		/// 处理图集
		/// </summary>
		private void OnPreprocessSpriteAtlas()
		{
			if (AssetImporterSettingData.Setting.Toggle == false)
				return;

			string importAssetPath = this.assetPath;
			IAssetProcessor processor = AssetImporterSettingData.GetCustomProcessor(importAssetPath);
			if (processor != null)
				processor.OnPreprocessSpriteAtlas(importAssetPath, this.assetImporter);
		}
		#endregion
	}
}