using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Look Inclination", story: "[Self] looks at [Player] with inclination on [animatorController]", category: "Action/Animation", id: "132bd057b39a56a6e7ef0c4d38ae6f37")]
public partial class LookInclinationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<Animator> AnimatorController;
    
    [SerializeReference] public BlackboardVariable<float> LookUpThreshold = new BlackboardVariable<float>(1.0f);
    [SerializeReference] public BlackboardVariable<float> LookDownThreshold = new BlackboardVariable<float>(1.0f);
    
    private const int LookUpLayerIndex = 3; // Assuming the look up layer is at index 1
    private const int LookDownLayerIndex = 4; // Assuming the look down layer is at index 2

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var selfTransform = Self.Value.transform;
        var playerTransform = Player.Value.transform;

        float deltaY = playerTransform.position.y - selfTransform.position.y;
        
        // Interpolate LookUp layer
        float lookUpWeight = Mathf.InverseLerp(0.5f, LookUpThreshold.Value, deltaY);
        
        var lookDownThreshold = LookDownThreshold.Value;
        
        float lookDownWeight = Mathf.InverseLerp(-0.5f, -lookDownThreshold, deltaY);


        AnimatorController.Value.SetLayerWeight(LookUpLayerIndex, Mathf.Lerp(AnimatorController.Value.GetLayerWeight(LookUpLayerIndex), lookUpWeight, 2f * Time.deltaTime));
        AnimatorController.Value.SetLayerWeight(
            LookDownLayerIndex,
            Mathf.Lerp(AnimatorController.Value.GetLayerWeight(LookDownLayerIndex), lookDownWeight, 2f * Time.deltaTime)
        );
        

        return Status.Running;
    }

    protected override void OnEnd()
    {
        AnimatorController.Value.SetLayerWeight(LookUpLayerIndex, 0f);
        
    }
}

