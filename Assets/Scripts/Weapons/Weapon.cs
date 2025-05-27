using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] protected WeaponTypes weaponType;
    [SerializeField] protected WeaponSocket socketToAttach;

    public WeaponSocket GetSocketToAttach { 
        get { return socketToAttach; }         
    }

    public WeaponTypes WeaponType
    {
        get { return weaponType; }
    }

    public void DisableWeapon()
    {
        gameObject.SetActive(false);
    }

    public void EnableWeapon()
    {
        gameObject.SetActive(true);
    }
}
