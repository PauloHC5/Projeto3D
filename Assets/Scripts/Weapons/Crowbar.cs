using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowbar : Weapon
{
    [Header("Crowbar Properties")]
    [SerializeField] private int damage = 25;
    [SerializeField] private Collider crowbarCollider;

    public void EnableCollision()
    {
        crowbarCollider.enabled = true;        
    }

    public void DisableCollision() 
    {
        crowbarCollider.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {        
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(damage, weaponType);            
        }
    }
}
