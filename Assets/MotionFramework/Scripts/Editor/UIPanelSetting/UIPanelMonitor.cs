//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEditor;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#endif

#if UNITY_2018_4 || UNITY_2019_4 || UNITY_2020_3
using UnityEditor.Experimental.SceneManagement;
#endif

namespace MotionFramework.Editor
{
	public class UIPanelMonitor : UnityEditor.Editor
	{
		[InitializeOnLoadMethod]
		static void StartInitializeOnLoadMethod()
		{
#if UNITY_2018_4_OR_NEWER
			PrefabStage.prefabSaving += OnPrefabSaving;
#else
			// 监听Inspector的Apply事件
			PrefabUtility.prefabInstanceUpdated = delegate (GameObject go)
			{
				UIManifest manifest = go.GetComponent<UIManifest>();
				if (manifest != null)
					UIPanelModifier.Refresh(manifest);
			};
#endif
		}

#if UNITY_2018_4_OR_NEWER
		static void OnPrefabSaving(GameObject go)
		{
			PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
			if (stage != null)
			{
				UnityEngine.UI.UIManifest manifest = go.GetComponent<UnityEngine.UI.UIManifest>();
				if (manifest != null)
					UIPanelModifier.Refresh(manifest);
			}
		}
#endif
	}
}