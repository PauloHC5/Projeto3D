using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun    
{
    [Header("Hitscan Properties")]
    [SerializeField] private int damage = 10;
    [SerializeField] private int pelletsPerShot = 5;
    [SerializeField] private float spreadAngle = 5f;
    [SerializeField] private LayerMask shootLayer;


    public override void Fire()
    {        
        StartCoroutine(BurstFire());
        ShootRaycast(damage, shootLayer);
        base.ShootRaycast(damage, shootLayer, gunRange * 2f);
        base.Fire();
        magAmmo--;
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < pelletsPerShot; i++)
        {
            ShootRaycast(damage / 2, shootLayer);     
            yield return new WaitForEndOfFrame();
        }        
    }

    public override void Reload()
    {
        base.Reload();
    }

    protected override void ShootRaycast(int damage, LayerMask shootLayer, float gunRange = default)
    {
        Vector3 direction = fireSocket.forward;
        direction.x += Random.Range(-spreadAngle, spreadAngle);
        direction.y += Random.Range(-spreadAngle, spreadAngle);

        float rayDistance = gunRange == default ? this.gunRange : gunRange;

        RaycastHit hit;
        if (Physics.Raycast(fireSocket.position, direction, out hit, rayDistance, shootLayer))
        {
            if (hit.collider.gameObject.GetComponent<Enemy>())
            {
                hit.collider.gameObject.GetComponent<Enemy>().TakeDamage(damage, weaponType);
            }

            if (impactVFX != null)
            {
                Instantiate(impactVFX, hit.point, Quaternion.LookRotation(hit.normal));

            }

            // Check if the object hit has rigidbody
            if (hit.rigidbody && !hit.collider.CompareTag("Enemy"))
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
