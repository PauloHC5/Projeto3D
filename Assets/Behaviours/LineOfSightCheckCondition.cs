using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Enemy line fo sight check", story: "[Enemy] can see [Target] ?", category: "Conditions", id: "26e3f3a02ac4cbcc7a3fd2934e3e519a")]
public partial class LineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Enemy> Enemy;
    [SerializeReference] public BlackboardVariable<GameObject> Target;    
    

    public override bool IsTrue()
    {
        return Enemy.Value.PerformDetection(Target) != null;        
    }

    //OnStart
    public override void OnStart()
    {        
    }
}
