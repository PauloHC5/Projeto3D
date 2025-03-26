using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Test", story: "Print [Target]", category: "Action", id: "ac370ad21990b86e4f4cfcce05f88e30")]
public partial class TestAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Target.Value) Debug.Log(Target.Value.name);
        return Status.Running;

    }

    protected override void OnEnd()
    {
    }
}

