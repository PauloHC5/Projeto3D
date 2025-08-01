using System;
using UnityEngine;

public class ACorngun : ProjectileGun, IChargeable
{
    [SerializeField] private GameObject superAcorngunProjectilePrefab; // Prefab for the super ACorngun projectile
    
    private readonly int Charge = Animator.StringToHash("Charge");

    private float originalProjectileForce, originalRecoilX, originalRecoilY, originalRecoilZ, originalSnappiness, originalReturnSpeed;

    private void Start()
    {
        originalProjectileForce = projectileForce; // Store the original projectile force        
        originalRecoilX = _recoilX; // Store the original recoil X value
        originalRecoilY = _recoilY; // Store the original recoil Y value
        originalRecoilZ = _recoilZ; // Store the original recoil Z value
        originalSnappiness = _snappiness; // Store the original snappiness value
        originalReturnSpeed = _returnSpeed; // Store the original return speed value
    }

    public override void Fire()
    {
        projectileForce = originalProjectileForce; // Reset projectile force to original value after firing
        base.Fire();        
        spawnedProjectile.transform.localScale = Vector3.one; // Reset the size of the projectile after firing
        _magAmmo--;
        SoundManager.PlayShootSound(_weaponType, _gunAudioSource); // Play the attack sound
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
            _canFire = _magAmmo > 0;
        }
    }

    public void PerformCharge(bool buttomPressed)
    {
        // TOBE IMPLEMENTED: Handle the charging logic for the ACorngun here
        // Example: play an animation or a specific visual effect
    }

    public void PerformSuperFire()
    {
        // Increase the projectile force and recoil values for super fire
        _recoilX *= 8f; // Increase the recoil X for super fire
        _recoilY *= 5f; // Increase the recoil Y for super fire
        _recoilZ *= 5f; // Increase the recoil Z for super fire
        _snappiness *= 2f; // Increase snappiness for super fire
        _returnSpeed /= 2f; // Increase return speed for super fire
        projectileForce *= 2f; // Increase the force for super fire
        
        if (superAcorngunProjectilePrefab)
        {
            base.Fire(superAcorngunProjectilePrefab);
        }
        else base.Fire();
        
        _magAmmo = 0;
        SoundManager.PlayRandomSFX(WorldSfxType.SUPERSHOOT, _gunAudioSource); // Play the super fire sound

        // Reset recoil values after firing
        _recoilX = originalRecoilX;
        _recoilY = originalRecoilY;
        _recoilZ = originalRecoilZ;
        _snappiness = originalSnappiness;
        _returnSpeed = originalReturnSpeed;
    }
}
