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
	/// 神经网络
	/// </summary>
	public class NeuralNetwork
	{
		private readonly List<NeuralLayer> _allLayer = new List<NeuralLayer>();
		private NeuralLayer _inputLayer;
		private NeuralLayer _outputLayer;

		private const float LearningRate = 0.1f;
		private const float Momentum = 0.9f;

		private readonly ENeuralAct _generalNeuralAct = ENeuralAct.BipolarSigmoid;
		private readonly ENeuralAct _outputNeuralAct = ENeuralAct.Logistic;

		public float Error = 0;


		/// <summary>
		/// 初始化神经网络
		/// </summary>
		/// <param name="inputLayerNodes">输入神经网络层的节点个数</param>
		/// <param name="outputLayerNodes">输出神经网络层的节点个数</param>
		/// <param name="hiddenLayerNodes">隐藏神经网络层的节点个数</param>
		/// <param name="hiddenLayers">隐藏神经网络层的数量</param>
		public void Init(int inputLayerNodes, int outputLayerNodes, int hiddenLayerNodes, int hiddenLayers)
		{
			// 清空
			_allLayer.Clear();
			_inputLayer = null;
			_outputLayer = null;

			int outputLayerInputs = inputLayerNodes;

			// 输入层
			_inputLayer = AddLayer(inputLayerNodes, 1, ENeuralLayerType.Input);

			// 隐藏层
			if (hiddenLayers > 0)
			{
				outputLayerInputs = hiddenLayerNodes;

				// first hidden layer connect back to inputs
				AddLayer(hiddenLayerNodes, inputLayerNodes, ENeuralLayerType.Hidden);

				for (int i = 0; i < hiddenLayers - 1; i++)
				{
					AddLayer(hiddenLayerNodes, hiddenLayerNodes, ENeuralLayerType.Hidden);
				}
			}

			// 输出层
			_outputLayer = AddLayer(outputLayerNodes, outputLayerInputs, ENeuralLayerType.Output);
		}

		/// <summary>
		/// 使用神经网络
		/// </summary>
		public void Use(List<float> inputs, List<float> outputs)
		{
			SetInputs(inputs);
			Propagate();
			outputs.Clear();

			// return the net outputs
			for (int i = 0; i < _outputLayer.Neurons.Count; ++i)
			{
				outputs.Add(_outputLayer.Neurons[i].Output);
			}
		}

		/// <summary>
		/// 训练神经网络
		/// </summary>
		public void Train(List<float> inputs, List<float> outputs)
		{
			SetInputs(inputs);
			Propagate();
			FindError(outputs);
			BackPropagate();
		}


		/// <summary>
		/// 添加一个神经网络层到神经网络
		/// </summary>
		private NeuralLayer AddLayer(int neurons, int inputs, ENeuralLayerType layerType)
		{
			NeuralLayer layer = new NeuralLayer(neurons, inputs, layerType);
			_allLayer.Add(layer);
			return layer;
		}

		/// <summary>
		/// 添加神经网络的输入数据
		/// </summary>
		private void SetInputs(List<float> inputs)
		{
			for (int i = 0; i < _inputLayer.Neurons.Count; i++)
			{
				_inputLayer.Neurons[i].Output = inputs[i];
			}
		}

		/// <summary>
		/// FindError
		/// </summary>
		private void FindError(List<float> outputs)
		{
			Error = 0;
			int numNeurons = _outputLayer.Neurons.Count;
			for (int i = 0; i < numNeurons; ++i)
			{
				float outputVal = _outputLayer.Neurons[i].Output;
				float error = outputs[i] - outputVal;
				switch (_generalNeuralAct)
				{
					case ENeuralAct.Tanh:
						_outputLayer.Neurons[i].Error = _outputLayer.DerTanh(outputVal) * error;
						break;
					case ENeuralAct.BipolarSigmoid:
						_outputLayer.Neurons[i].Error = _outputLayer.DerBipolarSigmoid(outputVal) * error;
						break;
					case ENeuralAct.Logistic:
						_outputLayer.Neurons[i].Error = _outputLayer.DerLogistic(outputVal) * error;
						break;
					case ENeuralAct.Linear:
						_outputLayer.Neurons[i].Error = outputVal * error;
						break;
					default:
						throw new Exception("Should never get here.");
				}
				// error calculation for the entire net
				Error += 0.5f * error * error;
			}
		}

		/// <summary>
		/// 正向传播
		/// </summary>
		private void Propagate()
		{
			for (int i = 0; i < _allLayer.Count - 1; i++)
			{
				ENeuralAct act = (_allLayer[i + 1].LayerType == ENeuralLayerType.Output) ? _outputNeuralAct : _generalNeuralAct;
				_allLayer[i].Propagate(act, _allLayer[i + 1]);
			}
		}

		/// <summary>
		///  反向传播
		/// </summary>
		private void BackPropagate()
		{
			// backprop the error
			for (int i = _allLayer.Count - 1; i > 0; i--)
			{
				_allLayer[i].BackPropagate(_generalNeuralAct, _allLayer[i - 1]);
			}

			// adjust the weights
			for (int i = 1; i < _allLayer.Count; i++)
			{
				_allLayer[i].AdjustWeights(_allLayer[i - 1], LearningRate, Momentum);
			}
		}
	}
}