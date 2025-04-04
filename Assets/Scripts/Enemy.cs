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
    [SerializeField] private float shotgunDeathImpulse = 20.0f; 
    [SerializeField] private float shotgunHitImpulse = 10.0f;
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

    private int Death = Animator.StringToHash("Death");
    private int Velocity = Animator.StringToHash("Velocity");
    private int React = Animator.StringToHash("React");
    private int WeaponIndex = Animator.StringToHash("WeaponIndex");

    private bool hasApplyiedDamageImpulse = false;
    private IEnumerator shotgunHitReactRoutine;

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
            if (shotgunHitReactRoutine != null) StopCoroutine(shotgunHitReactRoutine);
            Die(damageType);            
            return;
        }

        float layerWeight = (damageType == PlayerWeapon.Thompson || damageType == PlayerWeapon.Crossbow) ? mediumLayerWeight : fullLayerWeight;
        animator.SetLayerWeight(reactionLayerIndex, layerWeight);

        // Trigger the react animation based on the damage type
        // You can use an enum or int to represent different damage types
        animator.SetInteger(WeaponIndex, (int)damageType);


        if (damageType == PlayerWeapon.Shotgun)
        {
            if (shotgunHitReactRoutine == null)
            {
                shotgunHitReactRoutine = StunReact();
                StartCoroutine(shotgunHitReactRoutine);
            }            
        }
        else
            animator.SetTrigger(React);
    }   

    private void Die(PlayerWeapon damageType)
    {
        behaviorGraph.enabled = false;
        agent.enabled = false;
        enemyCollider.enabled = false;
        rb.isKinematic = false;
        animator.SetInteger(WeaponIndex, (int)damageType);
        animator.SetTrigger(Death);

        if (enemyDeadCollider) enemyDeadCollider.enabled = true;

        if (damageType == PlayerWeapon.Shotgun)
        {
            agent.velocity = Vector3.zero;
            ApplyShotgunImpulse(shotgunDeathImpulse);
        }
    }

    private IEnumerator StunReact()
    {                
        agent.velocity = Vector3.zero;
        agent.enabled = false;
        behaviorGraph.enabled = false;
        rb.isKinematic = false;
        ApplyShotgunImpulse(shotgunHitImpulse);
        animator.SetTrigger(React);
        yield return new WaitForSeconds(stunDuration);
        agent.enabled = true;
        behaviorGraph.enabled = true;
        behaviorGraph.Restart();
        rb.isKinematic = true;
    }

    private void ApplyShotgunImpulse(float shotgunImpulse)
    {                
        if(rb.isKinematic) Debug.LogWarning("Rigidbody is kinematic, cannot apply impulse.");

        // set the rotation of the enemy to look at the player
        Vector3 lookAtDirection = Camera.main.transform.position - transform.position;
        lookAtDirection.y = 0; // Keep the y component zero to only rotate on the y-axis
        Quaternion rotation = Quaternion.LookRotation(lookAtDirection);
        transform.rotation = rotation;

        // Apply impulse force to the enemy                        
        Vector3 direction = Camera.main.transform.forward;        
        rb.AddForce(direction * shotgunImpulse, ForceMode.Impulse);

        Debug.Log("Shotgun impulse applied to enemy.");
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
