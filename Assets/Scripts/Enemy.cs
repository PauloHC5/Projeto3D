using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Properties")]
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject weapon;

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
    private Rigidbody rb;

    private int Death = Animator.StringToHash("Death");
    private int Velocity = Animator.StringToHash("Velocity");
    private int React = Animator.StringToHash("React");
    private int WeaponIndex = Animator.StringToHash("WeaponIndex");

    private const int reactionLayerIndex = 1; // Index of the reaction layer in the animator
    private const float mediumLayerWeight = 0.75f; // Medium layer weight for the reaction layer
    private const float fullLayerWeight = 1.0f; // Full layer weight for the reaction layer

    public GameObject DetectedTarget { get; set; } // The detected target within the detection zone

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();        
        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        behaviorGraph.BlackboardReference.SetVariableValue("Speed", agent.speed);
    }

    private void Update()
    {
        animator.SetFloat(Velocity, Mathf.Clamp(agent.velocity.sqrMagnitude, 0f, 1f));        
    }

    // funtion to take damage
    public void TakeDamage(int damage, PlayerWeapon damageType)
    {        
        health -= damage;        

        if (health <= 0)
        {
            Die(damageType);            
        }

        float layerWeight = (damageType == PlayerWeapon.Thompson || damageType == PlayerWeapon.Crossbow) ? mediumLayerWeight : fullLayerWeight;
        animator.SetLayerWeight(reactionLayerIndex, layerWeight);

        // Trigger the react animation based on the damage type
        // You can use an enum or int to represent different damage types
        animator.SetInteger(WeaponIndex, (int)damageType);
        animator.SetTrigger(React);
    }

    private void Die(PlayerWeapon damageType)
    {
        behaviorGraph.enabled = false;
        agent.enabled = false;
        enemyCollider.enabled = false;
        animator.SetInteger(WeaponIndex, (int)damageType);
        animator.SetTrigger(Death);

        if (damageType == PlayerWeapon.Shotgun)
        {
            // Apply impulse force to the enemy            
            rb.isKinematic = false; // Ensure the Rigidbody is not kinematic
            rb.AddForce(Vector3.forward * 10f, ForceMode.Impulse);            
        }
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
