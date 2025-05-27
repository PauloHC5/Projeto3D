using System;
using UnityEngine;

public class ACorngun : ProjectileGun
{    

    public override void Fire()
    {        
        base.Fire();
        magAmmo--;

        if (magAmmo <= 0)
        {
            //PerformReload();
            canFire = false;
        }
    }

    public override void PerformReload()
    {
        base.PerformReload();
    }    
}
