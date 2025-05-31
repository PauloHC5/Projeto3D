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

    public override void Reload(ref int playerGunAmmo)
    {        
        if (playerGunAmmo > 0) // If the ammo to reload is greater than the player ammo
        {
            MagAmmo += 10; // Set the mag ammo to the ammo to reload
            playerGunAmmo -= 1; // Subtract the ammo from the player ammo
            canFire = magAmmo > 0;
        }                
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
