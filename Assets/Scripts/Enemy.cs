using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Properties")]
    [SerializeField] private int health = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject weapon;
    [SerializeField] private CapsuleCollider enemyDeadCollider;
    [SerializeField] private float deathImpulse = 20.0f; 
    [SerializeField] private float stunHitImpulse = 10.0f;
    [SerializeField] private float stunDuration = 0.5f;

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
    
    private int IsDead = Animator.StringToHash("IsDead");
    private int Velocity = Animator.StringToHash("Velocity");
    private int React = Animator.StringToHash("React");
    private int Stun = Animator.StringToHash("Stun");
    private int WeaponIndex = Animator.StringToHash("WeaponIndex");

    private bool isDead = false;
    private IEnumerator shotgunStunReactRoutine;

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
        animator.SetBool(IsDead, isDead);
    }

    // funtion to take damage
    public void TakeDamage(int damage, PlayerWeapon damageType)
    {
        if(isDead) return; // Ignore damage if already dead

        health -= damage;        

        if (health <= 0)
        {
            isDead = true;
            if (shotgunStunReactRoutine != null) StopCoroutine(shotgunStunReactRoutine);
            Die(damageType);                        
        }
        else
        {
            float layerWeight = (damageType == PlayerWeapon.Thompson || damageType == PlayerWeapon.Crossbow) ? mediumLayerWeight : fullLayerWeight;
            animator.SetLayerWeight(reactionLayerIndex, layerWeight);

            // Trigger the react animation based on the damage type
            // You can use an enum or int to represent different damage types
            animator.SetInteger(WeaponIndex, (int)damageType);


            if (damageType == PlayerWeapon.Shotgun)
            {
                if (shotgunStunReactRoutine == null)
                {
                    shotgunStunReactRoutine = StunReact();
                    StartCoroutine(shotgunStunReactRoutine);
                    return;
                }
            }            
            
            animator.SetTrigger(React);
        }        
    }   

    private void Die(PlayerWeapon damageType)
    {        
        enemyCollider.enabled = false;
        behaviorGraph.enabled = false;
        agent.enabled = false;        
        rb.isKinematic = false;
        animator.SetInteger(WeaponIndex, (int)damageType);        

        if (enemyDeadCollider) enemyDeadCollider.enabled = true;

        if (damageType == PlayerWeapon.Shotgun)
        {
            agent.velocity = Vector3.zero;
            ApplyImpulse(deathImpulse);
        }
    }

    private IEnumerator StunReact()
    {                        
        agent.velocity = Vector3.zero;
        agent.enabled = false;
        behaviorGraph.enabled = false;
        rb.isKinematic = false;
        ApplyImpulse(stunHitImpulse);
        animator.SetTrigger(Stun);
        yield return new WaitForSeconds(stunDuration);
        agent.enabled = true;
        behaviorGraph.enabled = true;
        behaviorGraph.Restart();
        rb.isKinematic = true;
        shotgunStunReactRoutine = null; // Reset the coroutine reference
    }

    private void ApplyImpulse(float impulse)
    {                        
        // set the rotation of the enemy to look at the player
        Vector3 lookAtDirection = Camera.main.transform.position - transform.position;
        lookAtDirection.y = 0; // Keep the y component zero to only rotate on the y-axis
        Quaternion rotation = Quaternion.LookRotation(lookAtDirection);
        transform.rotation = rotation;

        // Apply impulse force to the enemy                        
        Vector3 direction = Camera.main.transform.forward;        
        rb.AddForce(direction * impulse, ForceMode.Impulse);        
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
