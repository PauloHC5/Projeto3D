using System;
using Enemies;
using UnityEngine;

public class Woodsman : Enemy
{
    [Header("Woodsman Properties")] 
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private GameObject weapon;

    private void Awake()
    {
        OnAwake();

        // Set the attack rate in the behavior graph blackboard
        if (!BehaviorGraph.BlackboardReference.SetVariableValue("AttackRate", attackRate))
        {
            Debug.LogWarning(
                "Woodsman: Blackboard variable 'AttackRate' not found. Please ensure it is set in the Behavior Graph.");
        }

        if (DamageColliderEvents != null)
        {
            // For single hit on enter
            DamageColliderEvents.onHitDamage += DamagePlayer;
        }
    }

    private void Update()
    {
        OnUpdate();
    }

    protected override void DamagePlayer(Collider other)
    {
        if (IsDead) return;

        var player = other.GetComponent<PlayerCharacter>();

        // Check if the player is not null
        if (player != null)
        {
            var playerCharacter = player.GetComponent<PlayerCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.Health -= damage;
            }
        }
    }

    protected override void Die(DamageType damageType)
    {
        base.Die(damageType);

        DropWeapon();
    }

    private void DropWeapon()
    {
        if (!weapon) return;

        var weaponRigidbody = weapon.GetComponent<Rigidbody>();
        if (weaponRigidbody == null)
            Debug.LogWarning("Woodsman: Weapon does not have a Rigidbody component.");
        else
            weaponRigidbody.isKinematic = false;


        var weaponCollider = weapon.GetComponent<Collider>();
        if (weaponCollider == null)
            Debug.LogWarning("Woodsman: Weapon does not have a Collider component.");
        else
            weaponCollider.enabled = true;

        // Desatch weapon from player
        if (weaponRigidbody && weaponCollider)
        {
            weapon.transform.SetParent(null);
        }
    }

    private void OnDestroy()
    {
        DamageColliderEvents.onHitDamage -= DamagePlayer;
    }
}