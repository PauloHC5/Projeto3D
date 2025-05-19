using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Enemy Range Detector", story: "[Enemy] tries to find [Target]", category: "Action", id: "dd4de42501150a4698bc0599ee762a74")]
public partial class EnemyRangeDetectorAction : Action
{
    [SerializeReference] public BlackboardVariable<Enemy> Enemy;
    [SerializeReference] public BlackboardVariable<GameObject> Target;        

    protected override Status OnUpdate()
    {
        Target.Value = Enemy.Value.DetectPlayer();

        return Target.Value != null ? Status.Success : Status.Failure;
    }   
}

