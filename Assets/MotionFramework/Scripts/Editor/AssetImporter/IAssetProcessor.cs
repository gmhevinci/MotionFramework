//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEditor;

namespace MotionFramework.Editor
{
	/// <summary>
	/// 资源处理器接口
	/// </summary>
	public interface IAssetProcessor
	{
		void OnPreprocessModel(string importAssetPath, AssetImporter assetImporter);
		void OnPreprocessTexture(string importAssetPath, AssetImporter assetImporter);
		void OnPreprocessAudio(string importAssetPath, AssetImporter assetImporter);
		void OnPreprocessSpriteAtlas(string importAssetPath, AssetImporter assetImporter);
	}
}