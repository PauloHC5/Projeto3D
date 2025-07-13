using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PlayerWeaponTypes
{
    CARNIVOROUSPLANTS = 0,
    ACORNGUN = 1,
    BANANASHOTGUN = 2,
    CACTUSSCROSSBOW = 3,
}

public class Weapon : MonoBehaviour, IWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] protected PlayerWeaponTypes _weaponType;
    [SerializeField] protected WeaponSocket _socketToAttach;
    
    private bool _isEquipped = false; // Flag to check if the weapon is equipped
    protected Collider _triggerCollider; // Collider for weapon trigger

    public WeaponSocket SocketToAttach { 
        get => _socketToAttach;
        set => _socketToAttach = value;
    }

    public PlayerWeaponTypes WeaponType => _weaponType;

    public float WeaponRange => GetWeaponRange();
    
    protected void OnBaseAwake()
    {
        _triggerCollider = GetComponents<Collider>().FirstOrDefault(c => c.isTrigger);
        if (_triggerCollider == null)
        {
            Debug.LogError($"Weapon {gameObject.name} requires a Collider component for trigger detection.");
        }
    }

    protected virtual float GetWeaponRange()
    {
        return 0; // Default range, can be overridden by derived classes
    }

    public virtual void DisableWeapon()
    {
        gameObject.SetActive(false);
    }

    public virtual void EnableWeapon()
    {
        gameObject.SetActive(true);
    }

    public void AttatchToSocket(Transform rightHandSocket, Transform leftHandSocket)
    {
        transform.SetParent(_socketToAttach == WeaponSocket.RightHandSocket ? rightHandSocket : leftHandSocket);

        transform.localPosition = Vector3.zero; // Reset local position
        transform.localRotation = Quaternion.identity; // Reset local rotation
        
        _isEquipped = true; // Set the weapon as equipped
        _triggerCollider.enabled = false; // Enable the trigger collider when equipped
    }
}
