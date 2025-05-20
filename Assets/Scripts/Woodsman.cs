using UnityEngine;

public class Woodsman : Enemy
{
    [SerializeField] private Transform weaponSocket;

    protected override void Die(WeaponTypes damageType)
    {
        base.Die(damageType);

        if (weapon)
        {
            weapon.GetComponent<Rigidbody>().isKinematic = false;
            weapon.GetComponent<Collider>().enabled = true;            

            // Desatch weapon from player
            weapon.transform.SetParent(null);

            // Set position and rotation of the weapon to the world position and rotation
            weapon.transform.position = weaponSocket.position;
            weapon.transform.rotation = weapon.transform.rotation;
        }             
    }
}
