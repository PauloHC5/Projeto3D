using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Range Detector", story: "Use [Self] Range Detector to find [Target]", category: "Action", id: "dd4de42501150a4698bc0599ee762a74")]
public partial class RangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private RangeDetector Detector;

    protected override Status OnStart()
    {        
        Detector = Self.Value.GetComponent<RangeDetector>();        

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Target.Value = Detector.DetectPlayer();               

        return Target.Value != null ? Status.Success : Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

