using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateLocomotion", story: "Update [Self] Locomotion", category: "Action", id: "ffbc1c1cb27551c7e04d46d28e176d12")]
public partial class UpdateLocomotionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    private NavMeshAgent m_NavMeshAgent;
    private Animator m_Animator;
    
    private int Velocity = Animator.StringToHash("Velocity");

    protected override Status OnStart()
    {
        m_NavMeshAgent = Self.Value.GetComponentInChildren<NavMeshAgent>();
        m_Animator = Self.Value.GetComponentInChildren<Animator>();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {    
        m_Animator.SetFloat(Velocity, Mathf.Clamp(m_NavMeshAgent.velocity.sqrMagnitude, 0f, 1f));                
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

