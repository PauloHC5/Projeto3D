using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Enemy line fo sight condition", story: "[Enemy] can see [Target] ?", category: "Conditions", id: "26e3f3a02ac4cbcc7a3fd2934e3e519a")]
public partial class EnemyLineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<EnemyDetectionController> Enemy;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    public override bool IsTrue()
    {
        if (Enemy.Value == null)
        {
            Debug.LogWarning("EnemyDetectionController component not found on the enemy. " +
                             "Please ensure the enemy has an EnemyDetectionController component attached.");
            return false;
        }
        
        return Enemy.Value.PerformLineOfSightDetection(Target.Value);
    }
}
