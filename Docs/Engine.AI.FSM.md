### 有限状态机 (Engine.AI.FSM)

定义有限状态机或流程的节点
```C#
public class CustomNode : IFsmNode
{
	public string Name { private set; get; }

	public CustomNode()
	{
		Name = "Node";
	}

	void IFsmNode.OnEnter()
	{
	}
	void IFsmNode.OnUpdate()
	{
	}
	void IFsmNode.OnExit()
	{
	}
	void IFsmNode.OnHandleMessage(object msg)
	{
	}
}
```

创建有限状态机
```C#
using MotionFramework.AI;

public class Test
{
	// 有限状态机
	private FiniteStateMachine _fsm = new FiniteStateMachine();

	public void Start()
	{
		// 节点转换关系图	
		FsmGraph graph = new FsmGraph("globalNode");
		graph.AddTransition("Node1", new List<string>() {"Node2"})
		graph.AddTransition("Node2", new List<string>() {"Node3", "Node4"})
		graph.AddTransition("Node3", new List<string>() {"Node2"})
		graph.AddTransition("Node4", new List<string>() {"Node2"})

		// 注意：如果不想限制节点之间的转换规则，可以设为空
		_fsm.Graph = graph;	

		// 添加节点
		_fsm.AddNode(new CustomNode1());
		_fsm.AddNode(new CustomNode2());
		_fsm.AddNode(new CustomNode3());
		_fsm.AddNode(new CustomNode4());
		_fsm.AddNode(new GlobalNode());

		// 运行入口节点
		_fsm.Run("Node1");
	}

	public void Update
	{
		// 更新
		_fsm.Update();
	}
}
```

创建流程
```C#
using MotionFramework.AI;

public class Test
{
	private Procedure _procedure = new Procedure();

	public void Start()
	{
		// 添加节点
		// 注意：按照先后顺序添加流程节点
		_procedure.AddNode(new CustomNode1());
		_procedure.AddNode(new CustomNode2());
		_procedure.AddNode(new CustomNode3());
		_procedure.AddNode(new CustomNode4());

		// 运行流程
		_procedure.Run();
	}
	public void Update
	{
		// 更新
		_procedure.Update();
	}
}
```