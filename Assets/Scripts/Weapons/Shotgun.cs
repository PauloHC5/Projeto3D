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
        ShootRaycast();
        base.ShootRaycast(gunRange * 2f);
        base.Fire();
        magAmmo--;
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < pelletsPerShot; i++)
        {
            ShootRaycast();     
            yield return new WaitForEndOfFrame();
        }        
    }

    protected override void FinishReload()
    {
        base.FinishReload();
    }

    protected override void ShootRaycast(float gunRange = default)
    {
        Vector3 direction = fireSocket.forward;
        direction.x += Random.Range(-spreadAngle, spreadAngle);
        direction.y += Random.Range(-spreadAngle, spreadAngle);

        float rayDistance = gunRange == default ? this.gunRange : gunRange;

        RaycastHit hit;
        if (Physics.Raycast(fireSocket.position, direction, out hit, rayDistance))
        {
            // Lógica de impacto do projétil
            if (impactVFX != null)
            {
                Instantiate(impactVFX, hit.point, Quaternion.LookRotation(hit.normal));

            }

            // Check if the object hit has rigidbody
            if (hit.rigidbody)
            {
                ApplyImpulse(hit);
            }
        }

        if (DebugRaycast)
        {
            Debug.DrawRay(fireSocket.position, direction * rayDistance, Color.red, 5f);
        }        
    }
}
