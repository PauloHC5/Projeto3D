using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun
{
    public override void Fire()
    {
        ShootRaycast();
        base.Fire();
    }
}
