//--------------------------------------------------
// Motion Framework
// Copyright©2018-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.Collections.Generic;

namespace MotionFramework.AI
{
	/// <summary>
	/// 神经网络层
	/// </summary>
	public class NeuralLayer
	{
		private const float Threshold = 1.0f;

		/// <summary>
		/// 神经元集合
		/// </summary>
		public readonly List<Neuron> Neurons = new List<Neuron>();

		/// <summary>
		/// 网络层类型
		/// </summary>
		public ENeuralLayerType LayerType { get; private set; }


		/// <summary>
		/// 神经网络层
		/// </summary>
		/// <param name="neurons">神经网络层的神经元数量</param>
		/// <param name="inputs">神经元接收的数据数量</param>
		/// <param name="layerType">神经网络层的类型</param>
		public NeuralLayer(int neurons, int inputs, ENeuralLayerType layerType)
		{
			LayerType = layerType;

			// 清空
			Neurons.Clear();

			// 创建神经元
			for (int i = 0; i < neurons; i++)
			{
				Neuron neuron = new Neuron(inputs);
				Neurons.Add(neuron);
			}
		}

		/// <summary>
		/// 正向传播
		/// </summary>
		public void Propagate(ENeuralAct neuralACT, NeuralLayer nextLayer)
		{
			int numNeurons = nextLayer.Neurons.Count;
			for (int i = 0; i < numNeurons; ++i)
			{
				float value = 0.0f;

				int numWeights = Neurons.Count;
				for (int j = 0; j < numWeights; ++j)
				{
					// sum the (weights * inputs), the inputs are the outputs of the prop layer
					value += nextLayer.Neurons[i].Weights[j] * Neurons[j].Output;
				}

				// add in the bias (always has an input of -1)
				value += nextLayer.Neurons[i].Weights[numWeights] * -1.0f;

				// store the outputs, but run activation first
				switch (neuralACT)
				{
					case ENeuralAct.Step:
						nextLayer.Neurons[i].Output = ActStep(value);
						break;
					case ENeuralAct.Tanh:
						nextLayer.Neurons[i].Output = ActTanh(value);
						break;
					case ENeuralAct.Logistic:
						nextLayer.Neurons[i].Output = ActLogistic(value);
						break;
					case ENeuralAct.BipolarSigmoid:
						nextLayer.Neurons[i].Output = ActBipolarSigmoid(value);
						break;
					case ENeuralAct.Linear:
						nextLayer.Neurons[i].Output = value;
						break;
					default:
						throw new Exception("Should never get here.");
				}
			}

			//if you wanted to run the Softmax activation function, you
			//would do it here, since it needs all the output values
			//if you pushed all the outputs into a vector, you could...
			//outputs = ActSoftmax(outputs);
			//and then put the outputs back into the correct slots
		}

		/// <summary>
		/// 反向传播
		/// </summary>
		public void BackPropagate(ENeuralAct neuralACT, NeuralLayer nextLayer)
		{
			int numNeurons = nextLayer.Neurons.Count;
			for (int i = 0; i < numNeurons; ++i)
			{
				float outputVal = nextLayer.Neurons[i].Output;
				float error = 0;
				for (int j = 0; j < Neurons.Count; ++j)
				{
					error += Neurons[j].Weights[i] * Neurons[j].Error;
				}

				switch (neuralACT)
				{
					case ENeuralAct.Tanh:
						nextLayer.Neurons[i].Error = DerTanh(outputVal) * error;
						break;
					case ENeuralAct.Logistic:
						nextLayer.Neurons[i].Error = DerLogistic(outputVal) * error;
						break;
					case ENeuralAct.BipolarSigmoid:
						nextLayer.Neurons[i].Error = DerBipolarSigmoid(outputVal) * error;
						break;
					case ENeuralAct.Linear:
						nextLayer.Neurons[i].Error = outputVal * error;
						break;
					default:
						{
							throw new NotImplementedException();
						}
				}
			}
		}

		/// <summary>
		/// 修正权重值
		/// </summary>
		/// <param name="inputLayer">输入神经网络层</param>
		/// <param name="learningRate">默认值为0.1f</param>
		/// <param name="momentum">默认值为0.9f</param>
		public void AdjustWeights(NeuralLayer inputLayer, float learningRate, float momentum)
		{
			for (int i = 0; i < Neurons.Count; i++)
			{
				int numWeights = Neurons[i].Weights.Count;
				for (int j = 0; j < numWeights; ++j)
				{
					// bias weight always uses -1 output value
					float output = (j == (numWeights - 1)) ? -1.0f : inputLayer.Neurons[j].Output;
					float error = Neurons[i].Error;
					float delta = momentum * Neurons[i].LastDelta[j] + (1.0f - momentum) * learningRate * error * output;
					Neurons[i].Weights[j] += delta;
					Neurons[i].LastDelta[j] = delta;
				}
			}
		}

		// 激活函数
		private float ActLogistic(float value)
		{
			return (1.0f / (1.0f + UnityEngine.Mathf.Exp(-value * Threshold)));
		}
		private float ActStep(float value)
		{
			return ((value > Threshold) ? 1.0f : 0.0f);
		}
		private float ActTanh(float value)
		{
			return (UnityEngine.Mathf.Tan(value * Threshold));
		}
		private float ActBipolarSigmoid(float value)
		{
			return ((2.0f / (1.0f + UnityEngine.Mathf.Exp(-value * Threshold))) - 1.0f);
		}
		private void ActSoftmax(NeuralLayer outputs)
		{
			float total = 0.0f;
			for (int i = 0; i < Neurons.Count; ++i)
			{
				total = UnityEngine.Mathf.Exp(outputs.Neurons[i].Output);
			}
			for (int i = 0; i < Neurons.Count; ++i)
			{
				outputs.Neurons[i].Output = UnityEngine.Mathf.Exp(outputs.Neurons[i].Output) / total;
			}
		}

		// 反激活函数
		public float DerLogistic(float value)
		{
			return (value * Threshold * (1.0f - value));
		}
		public float DerTanh(float value)
		{
			return (1.0f - value * value);
		}
		public float DerBipolarSigmoid(float value)
		{
			return (0.5f * Threshold * (1.0f + value) * (1.0f - value));
		}
	}
}