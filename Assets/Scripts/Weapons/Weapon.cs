using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] protected PlayerWeaponTypes _weaponType;
    [SerializeField] protected WeaponSocket _socketToAttach;

    public WeaponSocket GetSocketToAttach { 
        get { return _socketToAttach; }         
    }

    public PlayerWeaponTypes WeaponType
    {
        get { return _weaponType; }
    }

    public float WeaponRange => GetWeaponRange();

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
}
