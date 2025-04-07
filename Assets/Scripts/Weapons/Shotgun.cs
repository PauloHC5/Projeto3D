using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : Gun    
{
    [Header("Shotgun Properties")]
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask shootLayer;
    [SerializeField] private float damageCapsuleRadius = 0.5f;    

    [Header("Burst Properties")]
    [SerializeField] private float burstRange = 10f;
    [SerializeField] private int pelletsPerShot = 5;
    [SerializeField] private float spreadAngle = 5f;    


    public override void Fire()
    {
        //base.ShootRaycast(damage, shootLayer, gunRange);
        base.ShootCapsuleCast(damage, shootLayer, damageCapsuleRadius, gunRange);
        base.Fire();
        StartCoroutine(BurstFire());
        magAmmo--;
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < pelletsPerShot; i++)
        {
            BurstRaycast(shootLayer, burstRange);     
            yield return new WaitForEndOfFrame();
        }        
    }

    public override void Reload()
    {
        base.Reload();
    }

    private void BurstRaycast(LayerMask shootLayer, float burstRange)
    {
        Vector3 direction = fireSocket.forward;
        direction.x += Random.Range(-spreadAngle, spreadAngle);
        direction.y += Random.Range(-spreadAngle, spreadAngle);        

        RaycastHit hit;
        if (Physics.Raycast(fireSocket.position, direction, out hit, burstRange, shootLayer))
        {            
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
            Debug.DrawRay(fireSocket.position, direction * burstRange, Color.red, 5f);
        }        
    }

    private void OnDrawGizmos()
    {
        if (DebugRaycast)
        {
            Vector3 point1, point2;            

            point1 = Camera.main.transform.position;
            point2 = point1 + Camera.main.transform.forward * gunRange;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point1, damageCapsuleRadius);
            Gizmos.DrawWireSphere(point2, damageCapsuleRadius);
            Gizmos.DrawLine(point1 + Vector3.up * damageCapsuleRadius, point2 + Vector3.up * damageCapsuleRadius);
            Gizmos.DrawLine(point1 - Vector3.up * damageCapsuleRadius, point2 - Vector3.up * damageCapsuleRadius);
            Gizmos.DrawLine(point1 + Vector3.right * damageCapsuleRadius, point2 + Vector3.right * damageCapsuleRadius);
            Gizmos.DrawLine(point1 - Vector3.right * damageCapsuleRadius, point2 - Vector3.right * damageCapsuleRadius);
        }
    }
}
