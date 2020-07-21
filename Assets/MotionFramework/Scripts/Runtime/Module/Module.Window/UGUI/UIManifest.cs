//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[DisallowMultipleComponent]
	public class UIManifest : MonoBehaviour
	{
		public List<string> CacheAtlasTags = new List<string>();
		public List<string> ElementPath = new List<string>();
		public List<Transform> ElementTrans = new List<Transform>();
		public List<GameObject> AttachPrefabs = new List<GameObject>();
		
		private readonly Dictionary<string, Transform> _runtimeDic = new Dictionary<string, Transform>(200);


		private void Awake()
		{
			if (Application.isPlaying)
				ConvertListToDic();
		}

		/// <summary>
		/// 游戏运行时把List内容存在字典里方便查询
		/// </summary>
		private void ConvertListToDic()
		{
			_runtimeDic.Clear();

			if (ElementPath.Count == 0)
				throw new Exception($"Fatal error : {this.gameObject.name} elementPath list is empty.");
			if (ElementTrans.Count == 0)
				throw new Exception($"Fatal error : {this.gameObject.name} elementTrans list is empty.");
			if (ElementPath.Count != ElementTrans.Count)
				throw new Exception($"Fatal error : {this.gameObject.name} elementTrans list and elementPath list must has same count.");

			for (int i = 0; i < ElementPath.Count; i++)
			{
				string path = ElementPath[i];
				Transform trans = ElementTrans[i];
				_runtimeDic.Add(path, trans);
			}
		}

		/// <summary>
		/// 克隆一个附加的预制体
		/// </summary>
		public GameObject CloneAttachPrefab(string name)
		{
			foreach (var obj in AttachPrefabs)
			{
				if (obj.name == name)
					return GameObject.Instantiate(obj);
			}
			Debug.LogWarning($"Not found attach prefab : {name}");
			return null;
		}

		/// <summary>
		/// 根据全路径获取UI元素
		/// </summary>
		public Transform GetUIElement(string path)
		{
			if (string.IsNullOrEmpty(path))
				return null;

			if (_runtimeDic.TryGetValue(path, out Transform result) == false)
			{
				Debug.LogWarning($"Not found ui element : {path}");
			}
			return result;
		}

		/// <summary>
		/// 根据全路径获取UI组件
		/// </summary>
		public Component GetUIComponent(string path, string typeName)
		{
			Transform element = GetUIElement(path);
			if (element == null)
				return null;

			Component component = element.GetComponent(typeName);
			if (component == null)
				Debug.LogWarning($"Not found ui component : {path}, {typeName}");
			return component;
		}

		/// <summary>
		/// 根据全路径获取UI组件
		/// </summary>
		public T GetUIComponent<T>(string path) where T : UnityEngine.Component
		{
			Transform element = GetUIElement(path);
			if (element == null)
				return null;

			Component component = element.GetComponent<T>();
			if (component == null)
				Debug.LogWarning($"Not found ui component : {path}, {typeof(T)}");
			return component as T;
		}
	}
}