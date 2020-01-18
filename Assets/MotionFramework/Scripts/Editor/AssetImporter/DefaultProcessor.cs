//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 默认的资源处理器
	/// </summary>
	public class DefaultProcessor : IAssetProcessor
	{
		private string GetTemplateAssetPath(string importAssetPath)
		{
			// 获取导入资源所在文件夹内的所有文件的GUID
			string folderPath = Path.GetDirectoryName(importAssetPath);
			string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { folderPath });
			if (guids.Length == 0)
				return string.Empty;

			// 我们以Project视图里文件夹内首个资源做为模板
			return AssetDatabase.GUIDToAssetPath(guids[0]);
		}
		private AssetImporter GetTemplateAssetImporter(string importAssetPath)
		{
			// 获取模板资源路径
			string templateAssetPath = GetTemplateAssetPath(importAssetPath);
			if (string.IsNullOrEmpty(templateAssetPath))
				return null;

			var templateImporter = AssetImporter.GetAtPath(templateAssetPath);
			if (templateImporter == null)
				Debug.LogError($"[DefaultProcessor] 模板资源导入器获取失败 : {templateAssetPath}");

			return templateImporter;
		}

		private void ProcessAllModel(ModelImporter templateImporter)
		{
			string folderPath = Path.GetDirectoryName(templateImporter.assetPath);
			string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Model}", new[] { folderPath });
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				if (Path.GetFileName(assetPath) == Path.GetFileName(templateImporter.assetPath))
					continue;
				AssetDatabase.ImportAsset(assetPath);
			}
		}
		private void ProcessAllTexture(TextureImporter templateImporter)
		{
			string folderPath = Path.GetDirectoryName(templateImporter.assetPath);
			string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.Texture}", new[] { folderPath });
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				if (Path.GetFileName(assetPath) == Path.GetFileName(templateImporter.assetPath))
					continue;
				AssetDatabase.ImportAsset(assetPath);
			}
		}
		private void ProcessAllAudio(AudioImporter templateImporter)
		{
			string folderPath = Path.GetDirectoryName(templateImporter.assetPath);
			string[] guids = AssetDatabase.FindAssets($"t:{EAssetSearchType.AudioClip}", new[] { folderPath });
			for (int i = 0; i < guids.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
				if (Path.GetFileName(assetPath) == Path.GetFileName(templateImporter.assetPath))
					continue;
				AssetDatabase.ImportAsset(assetPath);
			}
		}

		#region 接口方法
		public void OnPreprocessModel(string importAssetPath, AssetImporter assetImporter)
		{
			ModelImporter templateImporter = GetTemplateAssetImporter(importAssetPath) as ModelImporter;
			if (templateImporter == null)
				return;

			// 如果模板被更改，那么更新全部资源
			if (Path.GetFileName(importAssetPath) == Path.GetFileName(templateImporter.assetPath))
			{
				ProcessAllModel(templateImporter);
				return;
			}

			ModelImporter targetImporter = assetImporter as ModelImporter;
			ImporterCopyer.CopyModelImporter(targetImporter, templateImporter);
			Debug.Log($"[DefaultProcessor] 资源格式设置完毕 : {importAssetPath}");
		}
		public void OnPreprocessTexture(string importAssetPath, AssetImporter assetImporter)
		{
			TextureImporter templateImporter = GetTemplateAssetImporter(importAssetPath) as TextureImporter;
			if (templateImporter == null)
				return;

			// 如果模板被更改，那么更新全部资源
			if (Path.GetFileName(importAssetPath) == Path.GetFileName(templateImporter.assetPath))
			{
				ProcessAllTexture(templateImporter);
				return;
			}

			TextureImporter targetImporter = assetImporter as TextureImporter;
			ImporterCopyer.CopyTextureImporter(targetImporter, templateImporter);
			Debug.Log($"[DefaultProcessor] 资源格式设置完毕 : {importAssetPath}");
		}
		public void OnPreprocessAudio(string importAssetPath, AssetImporter assetImporter)
		{
			AudioImporter templateImporter = GetTemplateAssetImporter(importAssetPath) as AudioImporter;
			if (templateImporter == null)
				return;

			// 如果模板被更改，那么更新全部资源
			if (Path.GetFileName(importAssetPath) == Path.GetFileName(templateImporter.assetPath))
			{
				ProcessAllAudio(templateImporter);
				return;
			}

			AudioImporter targetImporter = assetImporter as AudioImporter;
			ImporterCopyer.CopyAudioImporter(targetImporter, templateImporter);
			Debug.Log($"[DefaultProcessor] 资源格式设置完毕 : {importAssetPath}");
		}
		public void OnPreprocessSpriteAtlas(string importAssetPath, AssetImporter assetImporter)
		{
			string templateAssetPath = GetTemplateAssetPath(importAssetPath);
			if (string.IsNullOrEmpty(templateAssetPath))
				throw new System.Exception($"图集资源模板获取失败：{importAssetPath}");

			SpriteAtlas template = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(templateAssetPath);
			SpriteAtlas target = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(importAssetPath);
			ImporterCopyer.CopySpriteAtlasSetting(target, template);
			Debug.Log($"[DefaultProcessor] 资源格式设置完毕 : {importAssetPath}");
		}
		#endregion
	}
}