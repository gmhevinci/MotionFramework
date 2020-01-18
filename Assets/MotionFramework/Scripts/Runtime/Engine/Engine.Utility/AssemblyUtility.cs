//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace MotionFramework.Utility
{
	public static class AssemblyUtility
	{
		private static readonly List<Type> _cacheTypes = new List<Type>();

		static AssemblyUtility()
		{
			_cacheTypes.Clear();

			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				_cacheTypes.AddRange(assembly.GetTypes());
			}
		}

		/// <summary>
		/// 获取带继承关系的所有类的类型
		/// <param name="parentType">父类类型</param> 
		/// </summary>
		public static List<Type> GetAssignableTypes(System.Type parentType)
		{
			List<Type> result = new List<Type>();
			for (int i = 0; i < _cacheTypes.Count; i++)
			{
				Type type = _cacheTypes[i];

				// 判断继承关系
				if (parentType.IsAssignableFrom(type))
				{
					if (type.Name == parentType.Name)
						continue;
					result.Add(type);
				}
			}
			return result;
		}

		/// <summary>
		/// 获取带属性标签的所有类的类型
		/// <param name="attributeType">属性类型</param>
		/// </summary>
		public static List<Type> GetAttributeTypes(System.Type attributeType)
		{
			List<Type> result = new List<Type>();
			for (int i = 0; i < _cacheTypes.Count; i++)
			{
				System.Type type = _cacheTypes[i];

				// 判断属性标签
				if (Attribute.IsDefined(type, attributeType))
				{
					result.Add(type);	
				}
			}
			return result;
		}

		/// <summary>
		/// 获取带继承关系和属性标签的所有类的类型
		/// </summary>
		/// <param name="parentType">父类类型</param>
		/// <param name="attributeType">属性类型</param>
		/// <returns></returns>
		public static List<Type> GetAssignableAttributeTypes(System.Type parentType, System.Type attributeType, bool checkError = true)
		{
			List<Type> result = new List<Type>();
			for (int i = 0; i < _cacheTypes.Count; i++)
			{
				Type type = _cacheTypes[i];

				// 判断属性标签
				if (Attribute.IsDefined(type, attributeType))
				{
					// 判断继承关系
					if (parentType.IsAssignableFrom(type))
					{
						if (type.Name == parentType.Name)
							continue;
						result.Add(type);
					}
					else
					{
						if(checkError)
							throw new Exception($"class {type} must inherit from {parentType}.");
					}
				}
			}
			return result;
		}
	}
}