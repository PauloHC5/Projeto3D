using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thompson : Gun
{    
    [SerializeField] private LayerMask shootLayer;

    public override void Fire()
    {
        ShootRaycast(shootLayer);
        base.Fire();
        magAmmo--;
    }            
}
