using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thompson : Gun
{
    public override void Fire()
    {
        ShootRaycast();
        base.Fire();
        magAmmo--;
    }        

    protected override void FinishReload()
    {
        base.FinishReload();
    }
}
