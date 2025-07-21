using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Properties")]
    [SerializeField] private int health = 100;
    [SerializeField] protected int damage = 10;
    [SerializeField] private CapsuleCollider enemyDeadCollider;
    [SerializeField] private float deathImpulse = 20.0f; 
    [SerializeField] private float stunHitImpulse = 10.0f;
    [SerializeField] private float stunDuration = 0.5f;
    [SerializeField] private bool canStun = true;    
    [SerializeField] private GameObject enemyEatenMesh;

    protected NavMeshAgent agent;
    protected BehaviorGraphAgent behaviorGraph;
    protected Animator animator;
    private Collider enemyCollider;
    private Rigidbody rb;
    protected AudioSource _audioSource;

    protected int IsDead = Animator.StringToHash("IsDead");
    protected int Velocity = Animator.StringToHash("Velocity");
    private int React = Animator.StringToHash("React");
    private int Stun = Animator.StringToHash("Stun");
    private int WeaponIndex = Animator.StringToHash("WeaponIndex");

    protected bool isDead = false;
    private IEnumerator shotgunStunReactRoutine;

    private const int reactionLayerIndex = 1; // Index of the reaction layer in the animator
    private const float mediumLayerWeight = 0.75f; // Medium layer weight for the reaction layer
    private const float fullLayerWeight = 1.0f; // Full layer weight for the reaction layer    

    public GameObject DetectedTarget { get; set; } // The detected target within the detection zone
    public int Damage => damage;

    private void Awake()
    protected void OnAwake()
    {
        agent = GetComponent<NavMeshAgent>();
        behaviorGraph = GetComponent<BehaviorGraphAgent>();
        animator = GetComponentInChildren<Animator>();
        enemyCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            Debug.LogWarning("ChemAgent: AudioSource component is missing. Please add one for sound effects.");

        if(!behaviorGraph.BlackboardReference.SetVariableValue("Speed", agent.speed))
        {
            Debug.LogWarning("Enemy: Blackboard variable 'Speed' not found. \n" +
                             "Please ensure it is set in the Behavior Graph or if the name has changed.");
        }
        
        if (!behaviorGraph.BlackboardReference.SetVariableValue("EnemyAnimator", animator))
        {
            Debug.LogWarning("Enemy: Blackboard variable 'EnemyAnimator' not found. \n" +
                             "Please ensure it is set in the Behavior Graph or if the name has changed.");
        }

        if (!behaviorGraph.BlackboardReference.SetVariableValue("DistanceThreshold", agent.stoppingDistance))
        {
            Debug.LogWarning("Enemy: Blackboard variable 'DistanceThreshold' not found. \n " +
                             "Please ensure it is set in the Behavior Graph or if the name has changed.");
        }
    }

    private void Update()
    protected void OnUpdate()
    {
        if(animator)
        {
            animator.SetFloat(Velocity, Mathf.Clamp(agent.velocity.sqrMagnitude, 0f, 1f));
            animator.SetFloat(Velocity, agent.velocity.sqrMagnitude);
            animator.SetBool(IsDead, isDead);
        }
    }

    // funtion to take damage
    public void TakeDamage(int damage, PlayerWeaponTypes damageType)
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
            float layerWeight = (damageType == PlayerWeaponTypes.CACTUSSCROSSBOW) ? mediumLayerWeight : fullLayerWeight;
            animator.SetLayerWeight(reactionLayerIndex, layerWeight);

            // Trigger the react animation based on the damage type
            // You can use an enum or int to represent different damage types
            animator.SetInteger(WeaponIndex, (int)damageType);


            if (canStun && damageType == PlayerWeaponTypes.BANANASHOTGUN)
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

    protected virtual void Die(PlayerWeaponTypes damageType)
    {        
        gameObject.tag = "Untagged"; // Remove the enemy tag to prevent further detectio

        enemyCollider.enabled = false;
        behaviorGraph.enabled = false;
        agent.enabled = false;        
        rb.isKinematic = false;
        animator.SetInteger(WeaponIndex, (int)damageType);

        if (damageType == PlayerWeaponTypes.CARNIVOROUSPLANTS)
        {
            if(enemyEatenMesh == null)
            {
                Debug.LogWarning("Enemy eaten mesh has not been assigned.");
                return;
            }            
            
            GameObject eatenMeshInstance = Instantiate(enemyEatenMesh, transform.position, this.transform.rotation);
            
            Destroy(gameObject);
        }

        if (enemyDeadCollider) enemyDeadCollider.enabled = true;

        if (damageType == PlayerWeaponTypes.BANANASHOTGUN)
        {
            agent.velocity = Vector3.zero;
            ApplyImpulse(deathImpulse);
        }        
    }

    private IEnumerator StunReact()
    {
        if(!canStun) yield break; // Exit if stun is not allowed

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
      
}
