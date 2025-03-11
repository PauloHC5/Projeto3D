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

    public override void Reload()
    {
        gunL.Reload();
        gunR.Reload();
    }

    private void OnDestroy()
    {
        if(gunR) Destroy(gunR.gameObject);
        if(gunL) Destroy(gunL.gameObject);
    }    
}
