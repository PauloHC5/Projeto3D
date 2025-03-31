using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Target is not valid", story: "Abort if [target] is null", category: "Conditions", id: "de2338a6b009e0b669a462a24db4e868")]
public partial class TargetIsNotValidCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    public override bool IsTrue()
    {
        return Target.Value == null ? true : false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
