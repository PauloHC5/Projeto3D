using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowbar : Weapon
{
    [SerializeField] private SphereCollider crowbarCollider;

    public void EnableCollision()
    {
        crowbarCollider.enabled = true;
    }

    public void DisableCollision() 
    {
        crowbarCollider.enabled = false;
    }
}
