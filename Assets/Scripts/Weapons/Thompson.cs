using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thompson : Gun
{
    [Header("Hitscan Properties")]
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask shootLayer;

    public override void Fire()
    {
        ShootRaycast(damage, shootLayer);
        base.Fire();
        magAmmo--;
    }            
}
