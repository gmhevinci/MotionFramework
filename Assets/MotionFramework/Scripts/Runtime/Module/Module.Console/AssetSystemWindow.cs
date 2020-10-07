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
		private class InfoWrapper : IReference, IComparer<InfoWrapper>, IComparable<InfoWrapper>
		{
			public string Info;
			public ELoaderStates LoadState;
			public int ProviderFailedCount;

			public void OnRelease()
			{
				Info = string.Empty;
				LoadState = ELoaderStates.None;
				ProviderFailedCount = 0;
			}
			public int CompareTo(InfoWrapper other)
			{
				return Compare(this, other);
			}
			public int Compare(InfoWrapper a, InfoWrapper b)
			{
				return string.CompareOrdinal(a.Info, b.Info);
			}
		}

		/// <summary>
		/// 加载器总数
		/// </summary>
		private int _loaderTotalCount = 0;

		/// <summary>
		/// 显示信息集合
		/// </summary>
		private List<InfoWrapper> _cacheInfos = new List<InfoWrapper>(1000);

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
				var element = _cacheInfos[i];
				if (element.LoadState == ELoaderStates.Fail || element.ProviderFailedCount > 0)
					ConsoleGUI.RedLable(element.Info);
				else
					ConsoleGUI.Lable(element.Info);
			}
			ConsoleGUI.EndScrollView();
		}

		private void FilterInfos()
		{
			// 回收引用
			ReferencePool.Release(_cacheInfos);

			// 清空列表
			_cacheInfos.Clear();

			// 绘制显示列表
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

				string info = $"资源名称：{loader.BundleInfo.BundleName}  资源版本：{loader.BundleInfo.Version}  引用计数：{loader.RefCount}";
				InfoWrapper element = ReferencePool.Spawn<InfoWrapper>();
				element.Info = info;
				element.LoadState = loader.States;
				element.ProviderFailedCount = loader.GetFailedProviderCount();

				// 添加到显示列表
				_cacheInfos.Add(element);
			}

			// 重新排序
			_cacheInfos.Sort();
		}
	}
}