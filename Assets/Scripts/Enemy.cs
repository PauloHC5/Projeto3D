using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Properties")]
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 10;

    [Header("Range Detector Properties")]
    [SerializeField] private float detectionRadius = 5.0f; // Radius of the detection zone
    [SerializeField] private LayerMask detectionLayer; // Layer mask to filter the detection to specific layers (e.g., Player layer)
    [SerializeField] private bool showDebugVisuals = true; // Show the detection zone in the editor

    [Header("Line of Sight Detector Properties")]
    [SerializeField] private float detectionRange = 10.0f;
    [SerializeField] private float detectionHeight = 3.0f;
    [SerializeField] private Transform raycastOrigin;        

    private NavMeshAgent agent;
    private BehaviorGraphAgent behaviorGraph;
    private Animator animator;
    private Collider enemyCollider;

    private int Death = Animator.StringToHash("Death");
    private int Velocity = Animator.StringToHash("Velocity");
    private int React = Animator.StringToHash("React");

    public GameObject DetectedTarget { get; set; } // The detected target within the detection zone

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();        
        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider>();

        behaviorGraph.BlackboardReference.SetVariableValue("Speed", agent.speed);
    }

    private void Update()
    {
        animator.SetFloat(Velocity, Mathf.Clamp(agent.velocity.sqrMagnitude, 0f, 1f));        
    }

    // funtion to take damage
    public void TakeDamage(int damage)
    {        
        health -= damage;
        animator.SetTrigger(React);
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        behaviorGraph.enabled = false;
        agent.enabled = false;
        enemyCollider.enabled = false;
        animator.SetTrigger(Death);
    }

    public GameObject DetectPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                DetectedTarget = hitCollider.gameObject;
                return DetectedTarget;
            }
            else
            {
                DetectedTarget = null;
            }
        }

        return DetectedTarget;
    }

    public GameObject PerformDetection(GameObject potentialTarget)
    {
        RaycastHit hit;
        Vector3 direction = potentialTarget.transform.position - raycastOrigin.position;
        direction.y += detectionHeight; // Adjust the direction to include detectionHeight

        // Project a raycast
        if (Physics.Raycast(raycastOrigin.position, direction, out hit, detectionRange, detectionLayer))
        {
            if (showDebugVisuals && this.enabled)
            {
                Debug.DrawRay(raycastOrigin.position, direction * detectionRange, Color.red);
            }

            if (hit.collider.gameObject == potentialTarget)
            {
                return hit.collider.gameObject;
            }
        }
        else
        {
            if (showDebugVisuals && this.enabled)
            {
                Debug.DrawRay(raycastOrigin.position, direction * detectionRange, Color.green);
            }
        }

        return null;
    }

    // Optional: Draw the detection sphere in the editor for visualization
    private void OnDrawGizmosSelected()
    {
        if (!showDebugVisuals) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
