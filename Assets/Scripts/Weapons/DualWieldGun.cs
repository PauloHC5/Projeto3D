using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WhichGun
{
    GunR,
    GunL
}

public class DualWieldGun : Gun, ISecondaryAction
{
    private Gun gunR;
    private Gun gunL;    

    public DualWieldGun Initialize(Gun GunR, Gun GunL)
    {
        this.gunR = GunR;
        this.gunL = GunL;

        return this;
    }

    public new bool CanFire(WhichGun whichGun)
    {
        return whichGun == WhichGun.GunR ? gunR.CanFire : gunL.CanFire;
    }

    public new WeaponSocket SocketToAttach(WhichGun whichGun)
    {
        return whichGun == WhichGun.GunR ? WeaponSocket.WEAPON_SOCKET_R : WeaponSocket.WEAPON_SOCKET_L;
    }

    public override void Fire()
    {        
        gunR.Fire();
    }

    public void FireL()
    {
        gunL.Fire();
    }

    public override void Reload()
    {
        gunL.Reload();
        gunR.Reload();
    }

    private void OnDestroy()
    {
        Destroy(gunR.gameObject);
        Destroy(gunL.gameObject);
    }

    public bool Perform()
    {
        if (gunR.CanFire)
        {
            gunR.Fire();
            return true;
        }
        return false;
    }
}
