using System;
using Enemies;
using UnityEngine;

public class ChemAgent : Enemy
{
    [Header("Chem Agent Properties")] [SerializeField]
    private float distanceToStartFire = 5f;
    [SerializeField] private ParticleSystem gunParticle;
    [SerializeField] protected Collider damageCollider;
    [SerializeField] private float damageInterval = 1.5f;
    
    private float _lastDamageTime = -Mathf.Infinity;
    private ChemAgentAnimationsController _chemAgentAnimationsController;

    private void Awake()
    {
        OnAwake();

        _chemAgentAnimationsController = new ChemAgentAnimationsController(EnemyAnimationsControlller.Animator, Agent);

        // Set the distance to start firing in the behavior graph blackboard
        if (!BehaviorGraph.BlackboardReference.SetVariableValue("DistanceToStartFire", distanceToStartFire))
        {
            Debug.LogWarning(
                "ChemAgent: Blackboard variable 'DistanceToStartFire' not found. Please ensure it is set in the Behavior Graph.");
        }
        
        if (DamageColliderEvents != null)
        {
            // For single hit on enter
            DamageColliderEvents.onPersistanceDamage += DamagePlayer;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        OnUpdate();

        if (_chemAgentAnimationsController.IsFiring && !IsStunned)
        {
            if (!gunParticle.isPlaying) gunParticle.Play();
            if (damageCollider) damageCollider.enabled = true;

            if (AudioSource && !AudioSource.isPlaying) SoundManager.PlayRandomSFX(WorldSfxType.CHEMAGENT_GAS, AudioSource, true);
        }
        else
        {
            if (gunParticle.isPlaying) gunParticle.Stop();
            if (damageCollider) damageCollider.enabled = false;
            if (AudioSource) AudioSource.Stop();
        }
        
        if(IsStunned) _chemAgentAnimationsController.StopFire();
    }

    protected override void DamagePlayer(Collider other)
    {
        if (IsDead) return;
        
        if (Time.time - _lastDamageTime < damageInterval) return;
        _lastDamageTime = Time.time;

        var player = other.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            player.TakeDamage(damage, DamageType.Gas);
        }
        else
        {
            Debug.LogWarning("Woodsman: Player GameObject is null.");
        }
    }

    protected override void Die(DamageType damageType)
    {
        gunParticle.Stop();
        _chemAgentAnimationsController.IsFiring = false;

        base.Die(damageType);
        
        Agent.enabled = false;
        var enemyDetection = GetComponent<EnemyDetectionController>();
        if (enemyDetection) enemyDetection.enabled = false;
    }

    private void OnDestroy()
    {
        DamageColliderEvents.onPersistanceDamage -= DamagePlayer;
    }
}