using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check if Player is Alive", story: "[Player] is Alive ?", category: "Conditions", id: "88aca4e3f1838d9f9f79dd774ec0d612")]
public partial class CheckIfPlayerIsAliveCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    public override bool IsTrue()
    {
        return Player.Value != null && Player.Value.GetComponentInParent<PlayerCharacter>() != null && Player.Value.GetComponentInParent<PlayerCharacter>().Health > 0;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
