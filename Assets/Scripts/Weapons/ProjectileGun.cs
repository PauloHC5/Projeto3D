using UnityEngine;

public class ProjectileGun : Gun
{
    [Header("Projectile Gun Properties")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float projectileForce = 10f;

    public override void Fire()
    {        
        base.Fire();
        
        ShootProjectile();
    }

    protected void ShootProjectile()
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
        spawnedProjectile.GetComponent<Rigidbody>().AddForce(ray.direction * projectileForce, ForceMode.Impulse);
    }
}
