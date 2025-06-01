using UnityEngine;

public class RaycastGun : Gun
{
    [Header("Raycast Gun properties")]
    [SerializeField] private int damage = 10;
    [SerializeField] protected float gunRange = 100f;
    [SerializeField] protected LayerMask shootLayer;
    [SerializeField] private float shootImpactImpulse = 10f;
    [SerializeField] protected bool DebugRaycast = false;

    [Header("Raycast Gun VFX")]
    [SerializeField] protected ParticleSystem impactVFX;

    protected override float GetWeaponRange()
    {
        return gunRange;
    }

    public override void Fire()
    {
        base.Fire();
    }

    protected virtual void ShootRaycast()
    {
        Ray ray;
        if (fireSocket == null)
        {
            ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            ray = new Ray(fireSocket.position, Camera.main.transform.forward);
        }

        float rayDistance = gunRange == default ? this.gunRange : gunRange;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance, shootLayer))
        {
            if (hit.collider.gameObject.GetComponent<Enemy>())
            {
                hit.collider.gameObject.GetComponent<Enemy>().TakeDamage(damage, weaponType);
            }

            if (impactVFX) Instantiate(impactVFX, hit.point, Quaternion.LookRotation(-ray.direction));

            // Check if the object hit has rigidbody
            if (hit.rigidbody && !hit.collider.CompareTag("Enemy"))
            {
                ApplyImpulse(hit);
            }
        }

        if (DebugRaycast) Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 5f);
    }

    protected void ShootCapsuleCast(float radius)
    {
        Vector3 point1, point2;

        point1 = Camera.main.transform.position;
        point2 = point1 + Camera.main.transform.forward * gunRange;

        // Use OverlapCapsule to detect all colliders within the capsule's area
        Collider[] hitColliders = Physics.OverlapCapsule(point1, point2, radius, shootLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.GetComponent<Enemy>())
            {
                hitCollider.gameObject.GetComponent<Enemy>().TakeDamage(damage, weaponType);
            }
        }
    }

    protected void ApplyImpulse(RaycastHit hit)
    {
        float distance = hit.distance;
        float shootImpulse = Mathf.Lerp(shootImpactImpulse, shootImpactImpulse / 2f, distance / gunRange);
        hit.rigidbody.AddForce(-hit.normal * shootImpulse, ForceMode.Impulse);
    }    
}
