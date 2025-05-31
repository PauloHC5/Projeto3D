using System;
using UnityEngine;

public class ACorngun : ProjectileGun, IChargeable
{    
    private readonly int Charge = Animator.StringToHash("Charge");

    public override void Fire()
    {        
        base.Fire();
        magAmmo--;        
    }
    
    public override void PerformReload()
    {
        base.PerformReload();
    }
    
    public void PerformCharge(bool buttomPressed)
    {
        gunAnimator.SetBool(Charge, buttomPressed);
    }

    public void PerformSuperFire()
    {
        base.Fire();
        projectileForce *= 2f; // Double the force for super fire
        if (spawnedProjectile)
        {
            spawnedProjectile.transform.localScale *= 3f; // Double the size of the projectile
            spawnedProjectile.GetComponent<Projectile>().Damage = 100; // Set damage to 100 for super fire
        }
        magAmmo = 0;
    }
}
