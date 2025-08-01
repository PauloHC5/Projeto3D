using UnityEngine;

public class ProjectileGun : Gun
{
    [Header("Projectile Gun Properties")]
    [SerializeField] private GameObject projectilePrefab;    
    [SerializeField] protected float projectileForce = 10f;

    protected GameObject spawnedProjectile;

    protected override float GetWeaponRange()
    {
        return projectileForce > 0 ? Mathf.RoundToInt(projectileForce * 10) : 0; // Example range calculation based on projectile force
    }

    public override void Fire()
    {        
        base.Fire();
        
        ShootProjectile();
    }
    
    public virtual void Fire(GameObject projectilePrefab)
    {
        base.Fire();
        
        ShootProjectile(projectilePrefab);
    }
    
    protected void ShootProjectile()
    {
        if (!projectilePrefab || !_fireSocket)
        {
            Debug.LogWarning("FlareProjectile or FireSocket is not set.");
            return;
        }

        // Create a ray that points to the middle of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Instantiate the flare projectile at the spawn point
        spawnedProjectile = Instantiate(projectilePrefab, _fireSocket.position, Quaternion.LookRotation(ray.direction));

        // Apply force to the flare projectile
        spawnedProjectile.GetComponent<Rigidbody>().AddForce(ray.direction * projectileForce, ForceMode.Impulse);
    }

    protected void ShootProjectile(GameObject projectile)
    {
        if (projectile == null || _fireSocket == null)
        {
            Debug.LogWarning("Projectile or FireSocket is not set.");
            return;
        }

        // Create a ray that points to the middle of the screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Instantiate the flare projectile at the spawn point
        spawnedProjectile = Instantiate(projectile, _fireSocket.position, Quaternion.LookRotation(ray.direction));

        // Apply force to the flare projectile
        spawnedProjectile.GetComponent<Rigidbody>().AddForce(ray.direction * projectileForce, ForceMode.Impulse);
    }
}
