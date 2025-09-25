using System;
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
    private Collider _triggerCollider; // Collider for weapon trigger
    private PickupBehaviour _pickupBehaviour;
    private CapsuleCollider _dropColliderCapsule;
    private MeshCollider _dropMeshCollider;
    private Rigidbody _dropRigidbody;

    public WeaponSocket SocketToAttach { 
        get => _socketToAttach;
        set => _socketToAttach = value;
    }

    public PlayerWeaponTypes WeaponType => _weaponType;

    public float WeaponRange => GetWeaponRange();

    protected void OnBaseAwake()
    {
        _triggerCollider = GetComponent<Collider>();
        if (_triggerCollider == null)
        {
            _triggerCollider = GetComponentInParent<Collider>();
            if (_triggerCollider == null)
                Debug.LogWarning($"No collider found on {gameObject.name} or its parent. \n If this weapon is not going to be used as a pickup, you can ignore this warning.");
        }
        
        _pickupBehaviour = GetComponent<PickupBehaviour>();
        _dropColliderCapsule = GetComponent<CapsuleCollider>();
        _dropRigidbody = GetComponent<Rigidbody>();
        _dropMeshCollider = GetComponentInChildren<MeshCollider>();
    }
    
    public void DropWeapon()
    {
        if(_isEquipped)
        {
            transform.SetParent(null); // Detach from parent
            _isEquipped = false; // Set the weapon as not equipped
            
            if(_dropMeshCollider) _dropMeshCollider.enabled = true;
            else
            if(_dropColliderCapsule) _dropColliderCapsule.enabled = true;
            
            if (_dropRigidbody) 
            {
                _dropRigidbody.isKinematic = false; // Enable physics
                _dropRigidbody.AddForce(Vector3.forward * 2f, ForceMode.Impulse); // Add a slight forward force when dropped
            }
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
        if(_pickupBehaviour) _pickupBehaviour.enabled = false;
        
        transform.SetParent(_socketToAttach == WeaponSocket.RightHandSocket ? rightHandSocket : leftHandSocket);

        transform.localPosition = Vector3.zero; // Reset local position
        transform.localRotation = Quaternion.identity; // Reset local rotation
        
        _isEquipped = true; // Set the weapon as equipped
        if(_triggerCollider) _triggerCollider.enabled = false; // Enable the trigger collider when equippe
    }
}
