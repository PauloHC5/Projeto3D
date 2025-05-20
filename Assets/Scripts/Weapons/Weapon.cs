using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
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
}
