using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WhichGun
{
    GunR,
    GunL
}

public class DualWieldGun : Gun
{
    private Gun gunR;
    private Gun gunL;    

    public new int MagAmmo => gunR.MagAmmo + gunL.MagAmmo;

    public DualWieldGun Initialize(Gun GunR, Gun GunL)
    {
        if (GunR == null) throw new System.ArgumentNullException(nameof(GunR));
        if (GunL == null) throw new System.ArgumentNullException(nameof(GunL));

        this.gunR = GunR;
        this.gunL = GunL;

        return this;
    }

    public new bool CanFire(WhichGun whichGun)
    {
        if(magAmmo <= 0)
        {
            magAmmo = 0; // Prevent negative ammo
            return false;
        }
        return whichGun == WhichGun.GunR ? gunR.CanFire : gunL.CanFire;
    }

    public new WeaponSocket GetSocketToAttach(WhichGun whichGun)
    {
        return whichGun == WhichGun.GunR ? WeaponSocket.RightHand : WeaponSocket.LeftHand;
    }

    public override void Fire()
    {
        gunR.Fire();
        gunL.Fire();
    }

    public void Fire(WhichGun whichGun)
    {
        if (whichGun == WhichGun.GunL)
        {
            gunL.Fire();
        }
        else
        {
            gunR.Fire();
        }
    }

    public override void Reload(Dictionary<PlayerWeapon, Int32> weaponAmmo)
    {
        Debug.Log("Reloading dual wield gun");

        gunR.Reload(weaponAmmo);
        gunL.Reload(weaponAmmo);        
    }

    public override void PlayReload()
    {
        gunL.PlayReload();
        gunR.PlayReload();        
    }

    private void OnDestroy()
    {
        if(gunR) Destroy(gunR.gameObject);
        if(gunL) Destroy(gunL.gameObject);
    }

    public Gun GetGun(WhichGun gunR)
    {
        return gunR == WhichGun.GunR ? this.gunR : this.gunL;
    }

    private void OnEnable()
    {
        if (gunR) gunR.gameObject.SetActive(true);
        if (gunL) gunL.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if(gunR) gunR.gameObject.SetActive(false);
        if(gunL) gunL.gameObject.SetActive(false);
    }
}
