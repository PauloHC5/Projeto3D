using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun    
{
    [SerializeField] private int pelletsPerShot = 5;
    [SerializeField] private float spreadAngle = 5f;


    public override void Fire()
    {        
        StartCoroutine(BurstFire());
        base.Fire();
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < pelletsPerShot; i++)
        {
            ShootRaycast();     
            yield return new WaitForEndOfFrame();
        }
    }

    protected override void ShootRaycast()
    {
        Vector3 direction = fireSocket.forward;
        direction.x += Random.Range(-spreadAngle, spreadAngle);
        direction.y += Random.Range(-spreadAngle, spreadAngle);

        RaycastHit hit;
        if (Physics.Raycast(fireSocket.position, direction, out hit, gunRange))
        {
            // Lógica de impacto do projétil
            if (impactVFX != null)
            {
                Instantiate(impactVFX, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        if (DebugRaycast)
        {
            Debug.DrawRay(fireSocket.position, direction * gunRange, Color.red, 5f);
        }        
    }
}
