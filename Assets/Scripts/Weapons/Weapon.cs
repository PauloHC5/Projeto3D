using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Properties")]
    [SerializeField] protected PlayerWeapon weaponType;
    [SerializeField] protected WeaponSocket socketToAttach;

    public WeaponSocket GetSocketToAttach { 
        get { return socketToAttach; }         
    }

    public PlayerWeapon WeaponType
    {
        get { return weaponType; }
    }
}
