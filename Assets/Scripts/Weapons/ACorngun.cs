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
    
    public void PerformCharge(bool buttomPressed)
    {
        gunAnimator.SetBool(Charge, buttomPressed);
    }

    public void PerformSuperFire()
    {
        base.Fire();
        magAmmo = 0;
    }
}
