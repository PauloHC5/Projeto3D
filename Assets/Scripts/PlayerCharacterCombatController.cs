using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

[Serializable]
public struct WeaponAmmoPair
{
    public PlayerWeapon weapon;
    public Int32 ammo;
}

public class PlayerCharacterCombatController : PlayerCharacterMovementController
{
    [Space]
    [Header("Combat")]
    [SerializeField] protected PlayerWeapon weaponSelected;
    [SerializeField] protected Weapon[] weapons = new Weapon[5];
    [SerializeField] protected GameObject playerMesh;

    protected Weapon equippedWeapon;
    public Weapon EquippedWeapon => equippedWeapon;

    [SerializeField]
    private List<WeaponAmmoPair> weaponAmmoList = new List<WeaponAmmoPair>
    {
        new WeaponAmmoPair { weapon = PlayerWeapon.CROWBAR, ammo = 0 }, // Crowbar doesn't use ammo
        new WeaponAmmoPair { weapon = PlayerWeapon.PISTOL, ammo = 50 },
        new WeaponAmmoPair { weapon = PlayerWeapon.SHOTGUNS, ammo = 20 },
        new WeaponAmmoPair { weapon = PlayerWeapon.THOMPSOM, ammo = 300 },
        new WeaponAmmoPair { weapon = PlayerWeapon.CROSSBOW, ammo = 150 }
    };

    protected Dictionary<PlayerWeapon, Int32> weaponAmmo;

    protected void Awake()
    {        
        InitializeWeaponAmmo();        
    }

    private void InitializeWeaponAmmo()
    {
        weaponAmmo = weaponAmmoList.ToDictionary(pair => pair.weapon, pair => pair.ammo);
    }        

    protected virtual void SwitchToWeapon(PlayerWeapon weapon)
    {
        if (equippedWeapon) Destroy(equippedWeapon.gameObject);

        weaponSelected = weapon;
        Weapon weaponToSpawn = weapons[(int)weapon];

        if (weaponSelected == PlayerWeapon.SHOTGUNS) equippedWeapon = SetDualWieldGun(weaponToSpawn);
        else
        {
            Transform socketToAttach = playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(Component => Component.gameObject.tag.Equals(weaponToSpawn.GetSocketToAttach.ToString()));
            equippedWeapon = Instantiate(weaponToSpawn, socketToAttach);
        }

        if (equippedWeapon) equippedWeapon.transform.localPosition = Vector3.zero;

        PlaySwitchToWeapon(weapon);
    }

    private Gun SetDualWieldGun(Weapon weaponToSpawn)
    {
        DualWieldGun guns = new GameObject("DualWieldGun").AddComponent<DualWieldGun>();
        guns.transform.SetParent(transform);

        Transform socketRight = GetSocketTransform(guns.GetSocketToAttach(WhichGun.GunR));
        Transform socketLeft = GetSocketTransform(guns.GetSocketToAttach(WhichGun.GunL));

        guns.Initialize(
        (Gun)Instantiate(weaponToSpawn, socketRight),
        (Gun)Instantiate(weaponToSpawn, socketLeft)
        );

        return guns;
    }

    private Transform GetSocketTransform(WeaponSocket socket)
    {
        return playerMesh.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.gameObject.CompareTag(socket.ToString()));
    }

    protected void UseWeapon()
    {
        var equippedGun = equippedWeapon.GetComponent<Gun>();

        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            HandleDualWieldGunFire(equippedGuns, WhichGun.GunL);
            return;
        }
        else if (equippedGun)
        {
            if (!equippedGun.CanFire) return;
        }        

        PlayUseWeapon();
    }

    protected virtual void Reload()
    {
        if (equippedWeapon is DualWieldGun equippedGuns) HandleDualWieldGunReload(equippedGuns);

        else
        {



            Gun equippedGun = equippedWeapon as Gun;

            // If the Mag Ammo is equal to the Max Ammo, means that the gun is full
            // If the weapon ammo is less than or equal to 0, means that player has no ammo to reload
            if (equippedGun.MagAmmo == equippedGun.MaxAmmo || weaponAmmo[weaponSelected] <= 0) return;

            // Get the difference between the max ammo and the current ammo
            int ammoDifference = equippedGun.MaxAmmo - equippedGun.MagAmmo;

            // Get the ammo to reload
            Int32 ammoToReload = weaponAmmo[weaponSelected] >= ammoDifference ? ammoDifference : weaponAmmo[weaponSelected];

            //Subtract the ammo to reload from the weapon ammo
            weaponAmmo[weaponSelected] -= ammoToReload;
            //Add the ammo to reload to the gun AmmoToReload
            equippedGun.AmmoToReload = ammoToReload;

            // Debug the mag ammo and the weapon ammo
            Debug.Log($"AmmoToReload: {equippedGun.AmmoToReload} | Weapon Ammo: {weaponAmmo[weaponSelected]}");

            PlayReload();
        }
    }    

    protected void UseWeaponGadget()
    {
        if (equippedWeapon is DualWieldGun equippedGuns)
        {
            HandleDualWieldGunFire(equippedGuns, WhichGun.GunR);
            return;
        }

        equippedWeapon.GetComponent<ISecondaryAction>()?.Perform();
    }    

    private void HandleDualWieldGunFire(DualWieldGun equippedGuns, WhichGun whichGun)
    {
        if(whichGun == WhichGun.GunL && equippedGuns.CanFire(whichGun)) PlayUseWeapon(WhichGun.GunL);
        if(whichGun == WhichGun.GunR && equippedGuns.CanFire(whichGun)) PlayUseWeapon(WhichGun.GunR);
    }

    private void HandleDualWieldGunReload(DualWieldGun equippedGuns)
    {
        Gun[] gunsToReload = new Gun[] { equippedGuns.GetGun(WhichGun.GunR), equippedGuns.GetGun(WhichGun.GunL) };

        foreach(var gun in gunsToReload)
        {

            // If the Mag Ammo is equal to the Max Ammo, means that the gun is full
            // If the weapon ammo is less than or equal to 0, means that player has no ammo to reload
            if (gun.MagAmmo == gun.MaxAmmo || weaponAmmo[weaponSelected] <= 0) continue;

            // Get the difference between the max ammo and the current ammo
            int ammoDifference = gun.MaxAmmo - gun.MagAmmo;

            // Get the ammo to reload
            // If the weapon ammo is greater than or equal to the ammo difference, reload the ammo difference
            // Otherwise, reload the weapon ammo
            Int32 ammoToReload = weaponAmmo[weaponSelected] >= ammoDifference ? ammoDifference : weaponAmmo[weaponSelected];

            // Subtract the ammo to reload from the weapon ammo
            weaponAmmo[weaponSelected] -= ammoToReload;

            // Add the ammo to reload to the gun AmmoToReload
            gun.AmmoToReload = ammoToReload;

            Debug.Log($"AmmoToReload: {gun.AmmoToReload} | Weapon Ammo: {weaponAmmo[weaponSelected]}");

            PlayReload();
        }
    }
}
