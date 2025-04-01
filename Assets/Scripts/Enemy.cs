using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 10;          

    private NavMeshAgent agent;
    private BehaviorGraphAgent behaviorGraph;
    private Animator animator;
    private Collider collider;

    private int Death = Animator.StringToHash("Death");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();        
        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        animator = GetComponentInChildren<Animator>();
        collider = GetComponent<Collider>();

        behaviorGraph.BlackboardReference.SetVariableValue("Speed", agent.speed);
    }

    // funtion to take damage
    public void TakeDamage(int damage)
    {        
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        behaviorGraph.enabled = false;
        agent.enabled = false;
        collider.enabled = false;
        animator.SetTrigger(Death);
    }
}
