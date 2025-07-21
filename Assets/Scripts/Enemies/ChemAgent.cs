using System;
using UnityEngine;

public class ChemAgent : Enemy
{
    [Header("Chem Agent Properties")] [SerializeField]
    private float distanceToStartFire = 5f;
    [SerializeField] private ParticleSystem gunParticle;
    [SerializeField] protected Collider damageCollider;
    [SerializeField] private float damageInterval = 1.5f;
    
    private float _lastDamageTime = -Mathf.Infinity;

    private readonly int Fire = Animator.StringToHash("Fire");

    private void Awake()
    {
        OnAwake();

        // Set the distance to start firing in the behavior graph blackboard
        if (!behaviorGraph.BlackboardReference.SetVariableValue("DistanceToStartFire", distanceToStartFire))
        {
            Debug.LogWarning(
                "ChemAgent: Blackboard variable 'DistanceToStartFire' not found. Please ensure it is set in the Behavior Graph.");
        }
        
        if (_damageColliderEvents != null)
        {
            // For single hit on enter
            _damageColliderEvents.onPersistanceDamage += DamagePlayer;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        OnUpdate();

        if (animator.GetBool(Fire))
        {
            if (!gunParticle.isPlaying) gunParticle.Play();
            if (damageCollider) damageCollider.enabled = true;

            if (_audioSource) SoundManager.PlayRandomSFX(WorldSfxType.CHEMAGENT_GAS, _audioSource, true);
        }
        else
        {
            if (gunParticle.isPlaying) gunParticle.Stop();
            if (damageCollider) damageCollider.enabled = false;
            if (_audioSource) _audioSource.Stop();
        }
    }

    protected override void DamagePlayer(Collider other)
    {
        if (isDead) return;
        
        if (Time.time - _lastDamageTime < damageInterval) return;
        _lastDamageTime = Time.time;

        var player = other.GetComponent<PlayerCharacter>();
        if (player != null)
        {
            player.Health -= damage;
        }
        else
        {
            Debug.LogWarning("Woodsman: Player GameObject is null.");
        }
    }

    protected override void Die(PlayerWeaponTypes damageType)
    {
        gunParticle.Stop();
        animator.SetBool(Fire, false);

        base.Die(damageType);
    }

    private void OnDestroy()
    {
        _damageColliderEvents.onPersistanceDamage -= DamagePlayer;
    }
}