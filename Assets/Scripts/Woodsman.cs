using UnityEngine;

public class Woodsman : Enemy
{    
    protected override void Die(WeaponTypes damageType)
    {
        base.Die(damageType);

        if (weapon)
        {
            weapon.GetComponent<Rigidbody>().isKinematic = false;
            weapon.GetComponent<Collider>().enabled = true;            

            // Desatch weapon from player
            weapon.transform.SetParent(null);            
        }             
    }
}
