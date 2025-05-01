using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bolt : Projectile
{    
    private  void OnCollisionEnter(Collision collision)
    {                   
        rb.isKinematic = true;
        Woodsman enemy = collision.gameObject.GetComponent<Woodsman>();
        if (enemy)
        {
            enemy.TakeDamage(damage, PlayerWeapon.Crossbow);
            Destroy(gameObject);
        }
    }
}
