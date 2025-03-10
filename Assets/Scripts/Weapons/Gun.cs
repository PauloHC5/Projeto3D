using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{

    [Header("Gun Properties")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem impactVFX;

    public float FireRate { get { return fireRate; } }

    private bool canFire = true;
    public bool CanFire { get { return canFire; } }

    private Animator gunAnimator;

    private const string FireTrigger = "Fire";
    private const string ReloadTrigger = "Reload";

    private void Awake()
    {
        gunAnimator = GetComponent<Animator>();        
    }    

    public virtual void Fire()
    {
        if(muzzleFlash) muzzleFlash.Play();
        StartCoroutine(ShootDelay());        
        if(gunAnimator) gunAnimator.SetTrigger(FireTrigger);
    }

    public virtual void Reload()
    {
        gunAnimator.SetTrigger(ReloadTrigger);
    }

    private IEnumerator ShootDelay()
    {
        canFire = false;
        yield return new WaitForSeconds(FireRate);
        canFire = true;
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

    protected void ShootRaycast()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {                                    
            if (impactVFX) Instantiate(impactVFX, hit.point, Quaternion.LookRotation(-ray.direction));
        }        
    }

}

public interface ISecondaryAction
{
    bool Perform();
}
