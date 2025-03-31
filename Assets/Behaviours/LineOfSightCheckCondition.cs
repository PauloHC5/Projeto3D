using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of Sight check", story: "Check [Target] with [LineOfSightDetector]", category: "Conditions", id: "26e3f3a02ac4cbcc7a3fd2934e3e519a")]
public partial class LineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<LineOfSightDetector> LineOfSightDetector;

    public override bool IsTrue()
    {
        return LineOfSightDetector.Value.PerformDetection(Target) != null;
    }

    //OnStart
    public override void OnStart()
    {
        
    }
}
