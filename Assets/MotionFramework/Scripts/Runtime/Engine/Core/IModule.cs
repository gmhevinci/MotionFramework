//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework
{
	public interface IModule
	{
		/// <summary>
		/// 创建模块
		/// </summary>
		void OnCreate(System.Object createParam);

		/// <summary>
		/// 轮询模块
		/// </summary>
		void OnUpdate();

		/// <summary>
		/// GUI绘制
		/// </summary>
		void OnGUI();
	}
}