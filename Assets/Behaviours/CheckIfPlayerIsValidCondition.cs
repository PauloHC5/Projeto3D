using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check if Player is not valid or its dead", story: "[Player] is null or dead ?", category: "Variable Conditions", id: "89ea3fba946ae6b223ff59bb7de78c07")]
public partial class CheckIfPlayerIsValidCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    public override bool IsTrue()
    {
        return Player.Value == null || Player.Value.GetComponentInParent<PlayerCharacter>() == null || Player.Value.GetComponentInParent<PlayerCharacter>().Health <= 0;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
