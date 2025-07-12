using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{
    [Header("Weapon Properties")]
    [SerializeField] protected PlayerWeaponTypes _weaponType;
    [SerializeField] protected WeaponSocket _socketToAttach;

    public WeaponSocket SocketToAttach { 
        get { return _socketToAttach; }         
        set { _socketToAttach = value; }
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

    public void AttatchToSocket(Transform rightHandSocket, Transform leftHandSocket)
    {
        transform.SetParent(_socketToAttach == WeaponSocket.RightHandSocket ? rightHandSocket : leftHandSocket);

        transform.localPosition = Vector3.zero; // Reset local position
        transform.localRotation = Quaternion.identity; // Reset local rotation
    }
}
