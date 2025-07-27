using System.Collections;
using Enemies;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Properties")] [SerializeField]
    private int health = 100;

    [SerializeField] protected int damage = 10;
    [SerializeField] private GameObject enemyMesh;
    [SerializeField] private float deathImpulse = 20.0f;
    [SerializeField] private float stunHitImpulse = 10.0f;
    [SerializeField] private float stunDuration = 0.5f;
    [SerializeField] private bool canStun = true;
    [SerializeField] private GameObject enemyEatenMesh;

    protected NavMeshAgent Agent;
    protected BehaviorGraphAgent BehaviorGraph;
    protected EnemyAnimationsControlller EnemyAnimationsControlller;
    private Collider _enemyCollider;
    private Rigidbody _rb;
    protected AudioSource AudioSource;
    protected DamageColliderEvents DamageColliderEvents;
    private Collider _enemyMeshCollider;
    private Rigidbody _enemyMeshRigidbody;

    protected bool IsDead = false;
    private IEnumerator _shotgunStunReactRoutine;

    public int Health
    {
        get => health;
        set
        {
            health = Mathf.Clamp(value, 0, 100);
            if (health <= 0)
            {
                IsDead = true;
                if (_shotgunStunReactRoutine != null) StopCoroutine(_shotgunStunReactRoutine);
            }
        }
    }

    public GameObject DetectedTarget { get; set; } // The detected target within the detection zone
    public int Damage => damage;

    protected void OnAwake()
    {
        Agent = GetComponent<NavMeshAgent>();
        BehaviorGraph = GetComponent<BehaviorGraphAgent>();
        _enemyCollider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        AudioSource = GetComponent<AudioSource>();
        _enemyMeshCollider = enemyMesh.GetComponent<Collider>();
        _enemyMeshRigidbody = enemyMesh.GetComponent<Rigidbody>();
        

        EnemyAnimationsControlller = new EnemyAnimationsControlller(GetComponentInChildren<Animator>(true), Agent);

        if (AudioSource == null)
            Debug.LogWarning("ChemAgent: AudioSource component is missing. Please add one for sound effects.");

        if (!BehaviorGraph.BlackboardReference.SetVariableValue("Speed", Agent.speed))
        {
            Debug.LogWarning("Enemy: Blackboard variable 'Speed' not found. \n" +
                             "Please ensure it is set in the Behavior Graph or if the name has changed.");
        }

        if (!BehaviorGraph.BlackboardReference.SetVariableValue("EnemyAnimator", EnemyAnimationsControlller.Animator))
        {
            Debug.LogWarning("Enemy: Blackboard variable 'EnemyAnimator' not found. \n" +
                             "Please ensure it is set in the Behavior Graph or if the name has changed.");
        }

        if (!BehaviorGraph.BlackboardReference.SetVariableValue("DistanceThreshold", Agent.stoppingDistance))
        {
            Debug.LogWarning("Enemy: Blackboard variable 'DistanceThreshold' not found. \n " +
                             "Please ensure it is set in the Behavior Graph or if the name has changed.");
        }

        DamageColliderEvents = GetComponentInChildren<DamageColliderEvents>(true);
    }

    protected void OnUpdate()
    {
        EnemyAnimationsControlller.HandleLocomotion();
    }

    // funtion to take damage
    public void TakeDamage(int takenDamage, PlayerWeaponTypes damageType)
    {
        if (IsDead) return; // Ignore damage if already dead

        Health -= takenDamage;

        if (IsDead)
        {
            Die(damageType);
            return;
        }
        
        if (canStun && damageType == PlayerWeaponTypes.BANANASHOTGUN)
        {
            if (_shotgunStunReactRoutine == null)
            {
                _shotgunStunReactRoutine = StunReact();
                StartCoroutine(_shotgunStunReactRoutine);
                return;
            }
        }

        EnemyAnimationsControlller.PlayTakeDamage(damageType);
    }

    protected virtual void Die(PlayerWeaponTypes damageType)
    {
        gameObject.tag = "Untagged"; // Remove the enemy tag to prevent further detectio

        _enemyCollider.enabled = false;
        BehaviorGraph.enabled = false;
        _rb.isKinematic = false;
        
        EnemyAnimationsControlller.PlayDeath(damageType);

        if (damageType == PlayerWeaponTypes.CARNIVOROUSPLANTS)
        {
            if (enemyEatenMesh == null)
            {
                Debug.LogWarning("Enemy eaten mesh has not been assigned.");
                return;
            }

            GameObject eatenMeshInstance = Instantiate(enemyEatenMesh, transform.position, this.transform.rotation);

            Destroy(gameObject);
        }

        if (_enemyMeshCollider) _enemyMeshCollider.enabled = true;
        if (_enemyMeshRigidbody) _enemyMeshRigidbody.isKinematic = false;

        if (damageType == PlayerWeaponTypes.BANANASHOTGUN)
        {
            Agent.velocity = Vector3.zero;
            ApplyImpulse(deathImpulse);
        }
    }

    private IEnumerator StunReact()
    {
        if (!canStun) yield break; // Exit if stun is not allowed

        Agent.velocity = Vector3.zero;
        Agent.enabled = false;
        BehaviorGraph.enabled = false;
        _rb.isKinematic = false;
        ApplyImpulse(stunHitImpulse);
        EnemyAnimationsControlller.PlayStun();
        yield return new WaitForSeconds(stunDuration);
        Agent.enabled = true;
        BehaviorGraph.enabled = true;
        BehaviorGraph.Restart();
        _rb.isKinematic = true;
        _shotgunStunReactRoutine = null; // Reset the coroutine reference
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
        _rb.AddForce(direction * impulse, ForceMode.Impulse);
    }

    protected virtual void DamagePlayer(Collider other)
    {
        // This method should be overridden in derived classes to handle player damage
    }
}