//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------

namespace MotionFramework.AI
{
	/// <summary>
	/// 神经网络层的类型
	/// </summary>
	public enum ENeuralLayerType
	{
		Input,
		Hidden,
		Output
	}

	/// <summary>
	/// 神经细胞的激活函数
	/// </summary>
	public enum ENeuralAct
	{
		/// <summary>
		/// 罗吉斯函数（S型函数）
		/// </summary>
		Logistic,

		/// <summary>
		/// 阶跃函数
		/// </summary>
		Step,

		/// <summary>
		/// 正切函数
		/// </summary>
		Tanh,

		/// <summary>
		/// 双曲函数
		/// </summary>
		BipolarSigmoid,

		/// <summary>
		/// Softmax
		/// </summary>
		Softmax,

		/// <summary>
		/// 线性函数
		/// </summary>
		Linear
	}
}