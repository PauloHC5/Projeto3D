using System;
using UnityEngine;

public class ACorngun : ProjectileGun, IChargeable
{
    private readonly int Charge = Animator.StringToHash("Charge");

    private float originalProjectileForce, originalRecoilX, originalRecoilY, originalRecoilZ, originalSnappiness, originalReturnSpeed;

    private void Start()
    {
        originalProjectileForce = projectileForce; // Store the original projectile force        
        originalRecoilX = recoilX; // Store the original recoil X value
        originalRecoilY = recoilY; // Store the original recoil Y value
        originalRecoilZ = recoilZ; // Store the original recoil Z value
        originalSnappiness = snappiness; // Store the original snappiness value
        originalReturnSpeed = returnSpeed; // Store the original return speed value
    }

    public override void Fire()
    {
        projectileForce = originalProjectileForce; // Reset projectile force to original value after firing
        base.Fire();        
        spawnedProjectile.transform.localScale = Vector3.one; // Reset the size of the projectile after firing
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
        // Increase the projectile force and recoil values for super fire
        recoilX *= 8f; // Increase the recoil X for super fire
        recoilY *= 5f; // Increase the recoil Y for super fire
        recoilZ *= 5f; // Increase the recoil Z for super fire
        snappiness *= 2f; // Increase snappiness for super fire
        returnSpeed /= 2f; // Increase return speed for super fire
        projectileForce *= 2f; // Increase the force for super fire

        base.Fire();        
        if (spawnedProjectile)
        {
            spawnedProjectile.transform.localScale *= 3f; // Double the size of the projectile for super fire
            spawnedProjectile.GetComponent<Projectile>().Damage = 100; // Set damage to 100 for super fire
        }
        magAmmo = 0;

        // Reset recoil values after firing
        recoilX = originalRecoilX;
        recoilY = originalRecoilY;
        recoilZ = originalRecoilZ;
        snappiness = originalSnappiness;
        returnSpeed = originalReturnSpeed;
    }
}
