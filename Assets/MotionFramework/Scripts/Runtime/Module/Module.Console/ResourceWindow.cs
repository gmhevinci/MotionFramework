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

namespace MotionFramework.Console
{
	[ConsoleAttribute("资源系统", 104)]
	internal class ResourceWindow : IConsoleWindow
	{
		private class ProviderWrapper : IReference, IComparer<ProviderWrapper>, IComparable<ProviderWrapper>
		{
			public string AssetPath;
			public AssetProviderBase Provider;
			public readonly List<BundleDebugInfo> BundleDebugInfos = new List<BundleDebugInfo>();

			public void OnRelease()
			{
				AssetPath = null;
				Provider = null;

				ReferencePool.Release(BundleDebugInfos);
				BundleDebugInfos.Clear();
			}
			public int CompareTo(ProviderWrapper other)
			{
				return Compare(this, other);
			}
			public int Compare(ProviderWrapper a, ProviderWrapper b)
			{
				return string.CompareOrdinal(a.AssetPath, b.AssetPath);
			}
		}

		private readonly List<ProviderWrapper> _cacheProviders = new List<ProviderWrapper>(1000);

		// GUI相关
		private string _filterKey = string.Empty;
		private Vector2 _scrollPos = Vector2.zero;


		void IConsoleWindow.OnGUI()
		{
			// 过滤信息
			FilterInfos();

			GUILayout.BeginHorizontal();
			{
				GUILayout.Label("搜索关键字 : ", ConsoleGUI.LableStyle, GUILayout.Width(200));
				_filterKey = GUILayout.TextField(_filterKey, ConsoleGUI.TextFieldStyle, GUILayout.Width(500));
			}
			GUILayout.EndHorizontal();

			ConsoleGUI.Lable($"资源包加载器总数：{AssetSystem.GetLoaderCount()}");
			ConsoleGUI.Lable($"资源对象加载器总数：{AssetSystem.GetProviderCount()}");
			float offset = ConsoleGUI.ToolbarStyle.fixedHeight + ConsoleGUI.LableStyle.fontSize * 2;
			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos, offset);
			for (int i = 0; i < _cacheProviders.Count; i++)
			{
				var wrapper = _cacheProviders[i];
				string loaderInfo = $"Asset：{wrapper.AssetPath} 引用：{wrapper.Provider.RefCount}";
				if (wrapper.Provider.States == EAssetStates.Fail)
					ConsoleGUI.RedLable(loaderInfo);
				else
					ConsoleGUI.Lable(loaderInfo);
				foreach(var debugInfo in wrapper.BundleDebugInfos)
				{
					string bundleInfo = $"       --- Bundle：{debugInfo.BundleName}  引用：{debugInfo.RefCount} 版本：{debugInfo.Version}";
					if (debugInfo.States == ELoaderStates.Fail)
						ConsoleGUI.RedLable(bundleInfo);
					else
						ConsoleGUI.Lable(bundleInfo);
				}
			}
			ConsoleGUI.EndScrollView();
		}
		private void FilterInfos()
		{
			// 回收引用
			ReferencePool.Release(_cacheProviders);
			_cacheProviders.Clear();

			var providers = AssetSystem.GetAllProviders();
			foreach(var provider in providers)
			{
				// 只搜索关键字
				if (string.IsNullOrEmpty(_filterKey) == false)
				{
					if (provider.AssetPath.Contains(_filterKey) == false)
						continue;
				}

				ProviderWrapper wrapper = ReferencePool.Spawn<ProviderWrapper>();
				wrapper.AssetPath = provider.AssetPath;
				wrapper.Provider = provider;
				_cacheProviders.Add(wrapper);

				if (provider is AssetBundleProvider)
				{
					AssetBundleProvider temp = provider as AssetBundleProvider;
					temp.GetBundleDebugInfos(wrapper.BundleDebugInfos);
				}
				else if(provider is AssetBundleSubProvider)
				{
					AssetBundleSubProvider temp = provider as AssetBundleSubProvider;
					temp.GetBundleDebugInfos(wrapper.BundleDebugInfos);
				}
				else if(provider is SceneProvider)
				{
					SceneProvider temp = provider as SceneProvider;
					temp.GetBundleDebugInfos(wrapper.BundleDebugInfos);
				}
			}

			// 重新排序
			_cacheProviders.Sort();
		}
	}
}