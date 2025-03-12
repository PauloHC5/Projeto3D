using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{

    [Header("Gun Properties")]
    [SerializeField] protected float fireRate = 0.5f;
    [SerializeField] protected float gunRange = 100f;
    [SerializeField] protected float shootImpulse = 10f;
    [SerializeField] protected int maxAmmo = 40;
    [SerializeField] protected int magAmmo = 40;

    [Header("Gun Components")]
    [SerializeField] protected Transform fireSocket;
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected ParticleSystem impactVFX;    
    [SerializeField] protected bool DebugRaycast = false;  

    public float FireRate { get { return fireRate; } }

    private bool canFire = true;
    public bool CanFire { get { return canFire; } }

    private Animator gunAnimator;    

    private readonly int FireTrigger = Animator.StringToHash("Fire");
    private readonly int ReloadTrigger = Animator.StringToHash("Reload");

    private void Awake()
    {
        gunAnimator = GetComponent<Animator>();        
    }    

    public virtual void Fire()
    {
        if(muzzleFlash) muzzleFlash.Play();                
        if(gunAnimator) gunAnimator.SetTrigger(FireTrigger);
        magAmmo--;
        StartCoroutine(ShootDelay());
    }

    public virtual void Reload()
    {
        gunAnimator.SetTrigger(ReloadTrigger);
    }

    private void FinishReload()
    {
        magAmmo = maxAmmo;
    }

    protected IEnumerator ShootDelay()
    {
        canFire = false;
        yield return new WaitForSeconds(FireRate);
        canFire = magAmmo > 0 ? true : false;
    }

    protected void ShootProjectile(GameObject projectile, Transform spawnPoint, float impulse)
    {
        if (projectile == null || spawnPoint == null)
        {
            Debug.LogWarning("FlareProjectile or FlareSpawnPoint is not set.");
            return;
        }

        // Create a ray that points to the middle of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Instantiate the flare projectile at the spawn point
        GameObject spawnedProjectile = Instantiate(projectile, spawnPoint.position, Quaternion.LookRotation(ray.direction));        

        // Apply force to the flare projectile
        spawnedProjectile.GetComponent<Rigidbody>().AddForce(ray.direction * impulse, ForceMode.Impulse);
    }

    protected virtual void ShootRaycast(float gunRange = default)
    {
        Ray ray;
        if (fireSocket == default) {
            ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        }
        else
        {
            ray = new Ray(fireSocket.position, Camera.main.transform.forward);
        }

        float rayDistance = gunRange == default ? this.gunRange : gunRange;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, rayDistance))
        {                             
            if (impactVFX) Instantiate(impactVFX, hit.point, Quaternion.LookRotation(-ray.direction));
            
            // Check if the object hit has rigidbody
            if (hit.rigidbody)
            {
                ApplyImpulse(hit);
            }
        }

        if (DebugRaycast) Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red, 5f);
    }

    protected void ApplyImpulse(RaycastHit hit)
    {
        float distance = hit.distance;
        float shootImpulse = Mathf.Lerp(this.shootImpulse, this.shootImpulse / 2f, distance / gunRange);
        hit.rigidbody.AddForce(-hit.normal * shootImpulse, ForceMode.Impulse);
    }

}

public interface ISecondaryAction
{
    bool Perform();
}
