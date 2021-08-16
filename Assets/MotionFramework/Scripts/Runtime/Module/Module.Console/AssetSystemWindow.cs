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
	internal class AssetSystemWindow : IConsoleWindow
	{
		private class LoaderWrapper : IReference, IComparer<LoaderWrapper>, IComparable<LoaderWrapper>
		{
			public string BundleName;
			public BundleFileLoader Loader;

			public void OnRelease()
			{
				BundleName = null;
				Loader = null;
			}
			public int CompareTo(LoaderWrapper other)
			{
				return Compare(this, other);
			}
			public int Compare(LoaderWrapper a, LoaderWrapper b)
			{
				return string.CompareOrdinal(a.BundleName, b.BundleName);
			}
		}

		/// <summary>
		/// 加载器总数
		/// </summary>
		private int _loaderTotalCount = 0;

		/// <summary>
		/// 显示信息集合
		/// </summary>
		private List<LoaderWrapper> _cacheInfos = new List<LoaderWrapper>(1000);

		/// <summary>
		/// 过滤的关键字
		/// </summary>
		private string _filterKey = string.Empty;

		// GUI相关
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

			ConsoleGUI.Lable($"加载器总数：{_loaderTotalCount}");
			float offset = ConsoleGUI.ToolbarStyle.fixedHeight + ConsoleGUI.LableStyle.fontSize;
			_scrollPos = ConsoleGUI.BeginScrollView(_scrollPos, offset);
			for (int i = 0; i < _cacheInfos.Count; i++)
			{
				var loaderWrapper = _cacheInfos[i];

				string loaderInfo = $"名称：{loaderWrapper.BundleName}  版本：{loaderWrapper.Loader.BundleInfo.Version}  引用：{loaderWrapper.Loader.RefCount}";
				if (loaderWrapper.Loader.States == ELoaderStates.Fail)
					ConsoleGUI.RedLable(loaderInfo);
				else
					ConsoleGUI.Lable(loaderInfo);
			}
			ConsoleGUI.EndScrollView();
		}

		private void FilterInfos()
		{
			// 回收引用
			ReferencePool.Release(_cacheInfos);
			_cacheInfos.Clear();

			var fileLoaders = AssetSystem.GetAllLoaders();
			_loaderTotalCount = fileLoaders.Count;
			foreach (var loader in fileLoaders)
			{
				// 只搜索关键字
				if (string.IsNullOrEmpty(_filterKey) == false)
				{
					if (loader.BundleInfo.BundleName.Contains(_filterKey) == false)
						continue;
				}

				LoaderWrapper loaderWrapper = ReferencePool.Spawn<LoaderWrapper>();
				loaderWrapper.BundleName = loader.BundleInfo.BundleName;
				loaderWrapper.Loader = loader;

				// 添加到显示列表
				_cacheInfos.Add(loaderWrapper);
			}

			// 重新排序
			_cacheInfos.Sort();
		}
	}
}