//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using UnityEngine;
using UnityEditor;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace MotionFramework.Editor
{
	public class UIPanelMonitor : UnityEditor.Editor
	{
		[InitializeOnLoadMethod]
		static void StartInitializeOnLoadMethod()
		{
			PrefabStage.prefabSaving += OnPrefabSaving;
		}

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
	}
}