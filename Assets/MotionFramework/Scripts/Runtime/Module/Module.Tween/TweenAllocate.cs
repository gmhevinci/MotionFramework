//--------------------------------------------------
// Motion Framework
// Copyright©2021-2021 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using MotionFramework.Utility;

namespace MotionFramework.Tween
{
	public static class TweenAllocate
	{
		/// <summary>
		/// 并行执行的复合节点
		/// </summary>
		public static ParallelNode Parallel(params ITweenNode[] nodes)
		{
			ParallelNode sequence = new ParallelNode();
			sequence.AddNode(nodes);
			return sequence;
		}

		/// <summary>
		/// 顺序执行的复合节点
		/// </summary>
		public static SequenceNode Sequence(params ITweenNode[] nodes)
		{
			SequenceNode sequence = new SequenceNode();
			sequence.AddNode(nodes);
			return sequence;
		}

		/// <summary>
		/// 随机执行的复合节点
		/// </summary>
		public static SelectorNode Selector(params ITweenNode[] nodes)
		{
			SelectorNode sequence = new SelectorNode();
			sequence.AddNode(nodes);
			return sequence;
		}

		/// <summary>
		/// 执行节点
		/// </summary>
		public static ExecuteNode Execute(System.Action execute)
		{
			ExecuteNode node = new ExecuteNode
			{
				Execute = execute,
			};
			return node;
		}

		/// <summary>
		/// 条件等待节点
		/// </summary>
		public static UntilNode Until(System.Func<bool> condition)
		{
			UntilNode node = new UntilNode
			{
				Condition = condition,
			};
			return node;
		}

		/// <summary>
		/// 延迟计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="triggerCallback">触发事件</param>
		public static TimerNode Delay(float delay, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreateOnceTimer(delay);
			return new TimerNode(timer, triggerCallback);
		}

		/// <summary>
		/// 重复计时节点
		/// 注意：该节点为无限时长
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="triggerCallback">触发事件</param>
		public static TimerNode Repeat(float delay, float interval, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval);
			return new TimerNode(timer, triggerCallback);
		}

		/// <summary>
		/// 重复计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="duration">持续时间</param>
		/// <param name="triggerCallback">触发事件</param>
		/// <returns></returns>
		public static TimerNode Repeat(float delay, float interval, float duration, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval, duration);
			return new TimerNode(timer, triggerCallback);
		}

		/// <summary>
		/// 重复计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="interval">间隔时间</param>
		/// <param name="maxTriggerCount">最大触发次数</param>
		/// <param name="triggerCallback">触发事件</param>
		/// <returns></returns>
		public static TimerNode Repeat(float delay, float interval, long maxTriggerCount, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreatePepeatTimer(delay, interval, maxTriggerCount);
			return new TimerNode(timer, triggerCallback);
		}

		/// <summary>
		/// 持续计时节点
		/// </summary>
		/// <param name="delay">延迟时间</param>
		/// <param name="duration">持续时间</param>
		/// <param name="triggerCallback">触发事件</param>
		/// <returns></returns>
		public static TimerNode Duration(float delay, float duration, System.Action triggerCallback = null)
		{
			Timer timer = Timer.CreateDurationTimer(delay, duration);
			return new TimerNode(timer, triggerCallback);
		}
	}
}