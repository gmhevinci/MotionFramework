//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEngine.U2D;
using UnityEditor;
using UnityEditor.U2D;

namespace MotionFramework.Editor
{
	public static class ImporterCopyer
	{
		/// <summary>
		/// 复制模型导入器
		/// </summary>
		public static void CopyModelImporter(ModelImporter targetImporter, ModelImporter templateImporter)
		{
			//NOTE：Unity没有提供模型导入器的拷贝接口

			// Scene
			targetImporter.globalScale = templateImporter.globalScale;
			targetImporter.importBlendShapes = templateImporter.importBlendShapes;
			targetImporter.importVisibility = templateImporter.importVisibility;
			targetImporter.importCameras = templateImporter.importCameras;
			targetImporter.importLights = templateImporter.importLights;
			targetImporter.preserveHierarchy = templateImporter.preserveHierarchy;

			// Meshes
			targetImporter.meshCompression = templateImporter.meshCompression;
			targetImporter.isReadable = templateImporter.isReadable;
			targetImporter.optimizeGameObjects = templateImporter.optimizeGameObjects;
			targetImporter.addCollider = templateImporter.addCollider;

			// Geometry
			targetImporter.keepQuads = templateImporter.keepQuads;
			targetImporter.weldVertices = templateImporter.weldVertices;
			targetImporter.indexFormat = templateImporter.indexFormat;
			targetImporter.importBlendShapes = templateImporter.importBlendShapes;
#if UNITY_2018_4_OR_NEWER
			targetImporter.importBlendShapeNormals = templateImporter.importBlendShapeNormals;
#endif
			targetImporter.normalSmoothingAngle = templateImporter.normalSmoothingAngle;
#if UNITY_2018_4_OR_NEWER
			targetImporter.normalSmoothingSource = templateImporter.normalSmoothingSource;
#endif
			targetImporter.importTangents = templateImporter.importTangents;
			targetImporter.swapUVChannels = templateImporter.swapUVChannels;
			targetImporter.generateSecondaryUV = templateImporter.generateSecondaryUV;
			targetImporter.secondaryUVAngleDistortion = templateImporter.secondaryUVAngleDistortion;
			targetImporter.secondaryUVAreaDistortion = templateImporter.secondaryUVAreaDistortion;
			targetImporter.secondaryUVHardAngle = templateImporter.secondaryUVHardAngle;
			targetImporter.secondaryUVPackMargin = templateImporter.secondaryUVPackMargin;

			// Animation
			targetImporter.animationType = templateImporter.animationType;
		}

		/// <summary>
		/// 复制纹理导入器
		/// </summary>
		public static void CopyTextureImporter(TextureImporter targetImporter, TextureImporter templateImporter)
		{
			var recordBorder = targetImporter.spriteBorder;
			var recordPivot = targetImporter.spritePivot;

			// 通用属性
			TextureImporterSettings temper = new TextureImporterSettings();
			templateImporter.ReadTextureSettings(temper);
			targetImporter.SetTextureSettings(temper);
			targetImporter.spriteBorder = recordBorder;
			targetImporter.spritePivot = recordPivot;

			// 平台设置
			TextureImporterPlatformSettings platformSettingPC = templateImporter.GetPlatformTextureSettings("Standalone");
			TextureImporterPlatformSettings platformSettingIOS = templateImporter.GetPlatformTextureSettings("iPhone");
			TextureImporterPlatformSettings platformSettingAndroid = templateImporter.GetPlatformTextureSettings("Android");
			targetImporter.SetPlatformTextureSettings(platformSettingPC);
			targetImporter.SetPlatformTextureSettings(platformSettingIOS);
			targetImporter.SetPlatformTextureSettings(platformSettingAndroid);
		}

		/// <summary>
		/// 复制音频导入器
		/// </summary>
		public static void CopyAudioImporter(AudioImporter targetImporter, AudioImporter templateImporter)
		{
			// 通用属性
			targetImporter.forceToMono = templateImporter.forceToMono;
			targetImporter.loadInBackground = templateImporter.loadInBackground;
			targetImporter.ambisonic = templateImporter.ambisonic;
			targetImporter.defaultSampleSettings = templateImporter.defaultSampleSettings;

			// 注意：Normalize没有暴露的接口
			var templateObject = new SerializedObject(templateImporter);
			var templateProperty = templateObject.FindProperty("m_Normalize");
			var targetObject = new SerializedObject(targetImporter);
			var targetProperty = targetObject.FindProperty("m_Normalize");
			targetProperty.boolValue = templateProperty.boolValue;
			targetObject.ApplyModifiedProperties();

			// 平台设置
			AudioImporterSampleSettings sampleSettingsPC = templateImporter.GetOverrideSampleSettings("Standalone");
			AudioImporterSampleSettings sampleSettingsIOS = templateImporter.GetOverrideSampleSettings("iOS");
			AudioImporterSampleSettings sampleSettingsAndroid = templateImporter.GetOverrideSampleSettings("Android");
			targetImporter.SetOverrideSampleSettings("Standalone", sampleSettingsPC);
			targetImporter.SetOverrideSampleSettings("iOS", sampleSettingsIOS);
			targetImporter.SetOverrideSampleSettings("Android", sampleSettingsAndroid);
		}

		/// <summary>
		/// 复制图集设置
		/// </summary>
		public static void CopySpriteAtlasSetting(SpriteAtlas target, SpriteAtlas template)
		{
#if UNITY_2018_4_OR_NEWER
			// 注意：默认设置为False
			target.SetIncludeInBuild(false);

			// 通用属性
			target.SetPackingSettings(template.GetPackingSettings());
			target.SetTextureSettings(template.GetTextureSettings());

			// 平台设置
			target.SetPlatformSettings(template.GetPlatformSettings("Standalone"));
			target.SetPlatformSettings(template.GetPlatformSettings("iPhone"));
			target.SetPlatformSettings(template.GetPlatformSettings("Android"));
#else
		Debug.LogWarning($"{Application.unityVersion} is not support copy sprite atlas setting. Please upgrade to unity2018.4 or newer.");
#endif
		}
	}
}