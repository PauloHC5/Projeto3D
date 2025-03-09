using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponSocket
{
    WEAPON_SOCKET_R,
    WEAPON_SOCKET_L
}

public class Weapon : MonoBehaviour
{
    [Header("Weapon Properties")]
    [SerializeField] protected WeaponSocket socketToAttach;

    public WeaponSocket SocketToAttach { 
        get { return socketToAttach; }         
    }    
}
