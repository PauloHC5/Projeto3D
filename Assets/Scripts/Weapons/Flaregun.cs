using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flaregun : Gun
{
    [SerializeField] private GameObject flareProjectile;
    [SerializeField] private Transform flareSpawnPoint;
    [SerializeField] private float FlareForce = 100f;

    public override void Fire()
    {
        base.ShootProjectile(flareProjectile, flareSpawnPoint, FlareForce);
        base.Fire();
    }
}
