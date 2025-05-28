using System.Collections;
using UnityEngine;

public class DualWieldMeleeManager : IWeapon, IEquippedMelee
{
    public CarnivovrousPlant RightCarnivorousPlant { get; private set; }
    public CarnivovrousPlant LeftCarnivorousPlant { get; private set; }

    public WeaponTypes WeaponType => WeaponTypes.Melee;

    private bool toggleAttack = false;

    PlayerCharacterAnimationsController playerAnimationsController;

    public DualWieldMeleeManager(CarnivovrousPlant weapon, Transform rightWeaponSocketPos, Transform leftWeaponSocketPos, PlayerCharacterAnimationsController playerAnimationsController)
    {
        CarnivovrousPlant rightWeaponSpawned = Object.Instantiate(weapon, rightWeaponSocketPos);
        CarnivovrousPlant leftWeaponSpawned = Object.Instantiate(weapon, leftWeaponSocketPos);

        rightWeaponSpawned.DisableWeapon();
        leftWeaponSpawned.DisableWeapon();

        RightCarnivorousPlant = rightWeaponSpawned;
        LeftCarnivorousPlant = leftWeaponSpawned;

        this.playerAnimationsController = playerAnimationsController;
    }

    public void Attack()
    {
        toggleAttack = !toggleAttack;
        if (toggleAttack)
        {
            RightCarnivorousPlant.Attack();
        }
        else
        {
            LeftCarnivorousPlant.Attack();
        }

        playerAnimationsController.WeaponAltternation(toggleAttack);

    }

    public void EnableWeapon()
    {
        RightCarnivorousPlant.EnableWeapon();
        LeftCarnivorousPlant.EnableWeapon();
    }

    public void DisableWeapon()
    {
        RightCarnivorousPlant.DisableWeapon();
        LeftCarnivorousPlant.DisableWeapon();
    }    
}

public class DualWieldGunManager : IWeapon, IEquippedGun
{
    public Gun RightGun { get; private set; }
    public Gun LeftGun { get; private set; }

    public int MagAmmo => RightGun.MagAmmo + LeftGun.MagAmmo;

    public bool CanFire => (RightGun.CanFire && LeftGun.CanFire) && (RightGun.MagAmmo > 0 || LeftGun.MagAmmo > 0);        

    public int MagCapacity => RightGun.MagCapacity + LeftGun.MagCapacity;

    public WeaponTypes WeaponType => WeaponTypes.Shotgun;

    private bool toggleFire = false;
    public bool ToggleFire => toggleFire;

    int IEquippedGun.MagAmmo { get => RightGun.MagAmmo + LeftGun.MagAmmo;
        set 
        {
            RightGun.MagAmmo = value / 2;
        } 
    }

    PlayerCharacterAnimationsController playerAnimationsController;

    public bool CanReload() => RightGun.CanReload() || LeftGun.CanReload();

    public DualWieldGunManager(Gun gun, Transform rightGunSocketPos, Transform leftGunSocketPos, PlayerCharacterAnimationsController playerAnimationsController)
    {
        Gun rightGunSpawned = Object.Instantiate(gun, rightGunSocketPos);
        Gun leftGunSpawned = Object.Instantiate(gun, leftGunSocketPos);        

        RightGun = rightGunSpawned;
        LeftGun = leftGunSpawned;

        this.playerAnimationsController = playerAnimationsController;
    }

    public void Fire()
    {
        if (!CanFire || MagAmmo == 0) return;

        toggleFire = !toggleFire;

        if (toggleFire)
        {
            RightGun.Fire();
        }
        else
        {
            LeftGun.Fire();
        }

        playerAnimationsController.WeaponAltternation(toggleFire);
    }    

    public void FireBoth()
    {
        if (MagAmmo == 0) return;

        RightGun.DoubleRecoil();

        RightGun.Fire();        
        RightGun.Fire();        
        LeftGun.Fire();
        LeftGun.Fire();
    }

    public void PerformReload()
    {
        RightGun.PerformReload();
        LeftGun.PerformReload();
    }    

    public void EnableWeapon()
    {
        RightGun.EnableWeapon();
        LeftGun.EnableWeapon();        
    }

    public void DisableWeapon()
    {
        RightGun.DisableWeapon();
        LeftGun.DisableWeapon();        
    }    
}
