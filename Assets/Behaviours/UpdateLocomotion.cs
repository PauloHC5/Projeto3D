using UnityEngine;
using UnityEngine.AI;

public class UpdateLocomotion : MonoBehaviour
{

    private NavMeshAgent navMeshAgent;
    private Animator animator;

    private int Velocity = Animator.StringToHash("Velocity");

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {        
        animator.SetFloat(Velocity, Mathf.Clamp(navMeshAgent.velocity.sqrMagnitude, 0f, 1f));
    }
}
