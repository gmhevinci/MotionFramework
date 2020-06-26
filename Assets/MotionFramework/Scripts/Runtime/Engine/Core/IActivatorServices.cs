using System;

namespace MotionFramework
{
	public interface IActivatorServices
	{
		/// <summary>
		/// 创建实例
		/// </summary>
		object CreateInstance(Type classType);

		/// <summary>
		/// 获取特性
		/// </summary>
		Attribute GetAttribute(Type classType);
	}
}