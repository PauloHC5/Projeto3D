using System;
using UnityEngine;

public class ACorngun : ProjectileGun
{    

    public override void Fire()
    {        
        base.Fire();
        magAmmo--;        
    }

    public override void PerformReload()
    {
        base.PerformReload();
    }    
}
