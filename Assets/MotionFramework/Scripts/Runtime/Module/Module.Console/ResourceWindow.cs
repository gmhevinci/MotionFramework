//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework.Resource;
using MotionFramework.Reference;
using YooAsset;

namespace MotionFramework.Console
{
	[ConsoleAttribute("资源系统", 104)]
	internal class ResourceWindow : IConsoleWindow
	{
		// GUI相关
		private string _filterKey = string.Empty;
		private Vector2 _scrollPos = Vector2.zero;


		void IConsoleWindow.OnGUI()
		{
			DebugSummy summy = new DebugSummy();
			YooAssets.GetDebugSummy(summy);

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("搜索关键字 : ", ConsoleGUI.LableStyle, GUILayout.Width(200));
				_filterKey = GUILayout.TextField(_filterKey, ConsoleGUI.TextFieldStyle, GUILayout.Width(500));
			}
			GUILayout.EndHorizontal();

			ConsoleGUI.Lable($"资源包加载器总数：{summy.BundleCount}");
			ConsoleGUI.Lable($"资源对象加载器总数：{summy.AssetCount}");

			float offset = ConsoleGUI.ToolbarStyle.fixedHeight + ConsoleGUI.LableStyle.fontSize * 2;
			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos, offset);
			for (int i = 0; i < summy.ProviderInfos.Count; i++)
			{
				var providerInfo = summy.ProviderInfos[i];

				// 只搜索关键字
				if (string.IsNullOrEmpty(_filterKey) == false)
				{
					if (providerInfo.AssetPath.Contains(_filterKey) == false)
						continue;
				}

				GUILayout.Space(10);
				string loaderInfo = $"Asset：{providerInfo.AssetPath} 引用：{providerInfo.RefCount}";
				if (providerInfo.States == EAssetStates.Fail)
					ConsoleGUI.RedLable(loaderInfo);
				else
					ConsoleGUI.Lable(loaderInfo);

				bool showOwner = false;
				foreach(var bundleInfo in providerInfo.BundleInfos)
				{
					if(showOwner == false)
					{
						showOwner = true;
						string bundleContent = $"Bundle：{bundleInfo.BundleName}  引用：{bundleInfo.RefCount} 版本：{bundleInfo.Version}";
						if (bundleInfo.States == ELoaderStates.Fail)
							ConsoleGUI.RedLable(bundleContent);
						else
							ConsoleGUI.Lable(bundleContent);
					}
					else
					{
						string bundleContent = $"---Depend：{bundleInfo.BundleName}  引用：{bundleInfo.RefCount} 版本：{bundleInfo.Version}";
						if (bundleInfo.States == ELoaderStates.Fail)
							ConsoleGUI.RedLable(bundleContent);
						else
							ConsoleGUI.Lable(bundleContent);
					}
				}
			}
			ConsoleGUI.EndScrollView();
		}
	}
}