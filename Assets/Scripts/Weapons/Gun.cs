using System;
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

    [Header("Recoil Properties")]
    [SerializeField] private float recoilX = -2f;
    [SerializeField] private float recoilY = 2f;
    [SerializeField] private float recoilZ = 0.35f;
    [SerializeField] private float snappiness = 6f;
    [SerializeField] private float returnSpeed = 2f;

    [Header("Gun Components")]
    [SerializeField] protected Transform fireSocket;
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected ParticleSystem impactVFX;
    [SerializeField] protected bool DebugRaycast = false;

    private Int32 ammoToReload = 0;
    private bool canFire = true;    
    private CameraRecoil cameraRecoil;

    // Animator properties
    protected Animator gunAnimator;
    private readonly int FireTrigger = Animator.StringToHash("Fire");
    private readonly int ReloadTrigger = Animator.StringToHash("Reload");

    // Getters and Setters
    public Int32 AmmoToReload { get { return ammoToReload; } set { ammoToReload = value; } }
    public float FireRate { get { return fireRate; } }            
    public int MagAmmo { get => magAmmo; }    
    public int MaxAmmo => maxAmmo;

    public bool CanFire => canFire;


    protected virtual void Awake()
    {        
        gunAnimator = GetComponent<Animator>();
        if (gunAnimator == null) gunAnimator = GetComponentInChildren<Animator>();
        
        magAmmo = magAmmo > maxAmmo ? magAmmo = maxAmmo : magAmmo; // Clamp magAmmo to maxAmmo                

        cameraRecoil = Camera.main.GetComponentInParent<CameraRecoil>();             
    }

    public virtual void Fire()
    {
        if (gunAnimator) gunAnimator.SetTrigger(FireTrigger);
        else Debug.LogWarning("Gun animator not found.");

        if (muzzleFlash) muzzleFlash.Play();        
        if (cameraRecoil) cameraRecoil.RecoilFire(recoilX, recoilY, recoilZ, snappiness, returnSpeed);
        StartCoroutine(ShootDelay());
    }

    public virtual void PlayReload()
    {
        if (gunAnimator) gunAnimator.SetTrigger(ReloadTrigger);
        else Debug.LogWarning("Gun animator not found.");
    }

    public virtual void Reload()
    {
        var playerCharacterCombat = GameManager.Instance.Player.GetComponent<PlayerCharacterCombatController>();        

        // Get the difference between the max ammo and the current ammo
        int ammoDifference = MaxAmmo - MagAmmo;

        // Get the ammo to reload
        ammoToReload = playerCharacterCombat.WeaponAmmo[playerCharacterCombat.WeaponSelected] >= ammoDifference ? 
            ammoDifference : playerCharacterCombat.WeaponAmmo[playerCharacterCombat.WeaponSelected];

        // Subtract the ammo to reload from the weapon ammo
        playerCharacterCombat.WeaponAmmo[playerCharacterCombat.WeaponSelected] -= ammoToReload;

        magAmmo += AmmoToReload;
        canFire = true;        
    }

    protected IEnumerator ShootDelay()
    {
        canFire = false;
        yield return new WaitForSeconds(FireRate);
        canFire = magAmmo > 0;
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

    protected virtual void ShootRaycast(int damage, LayerMask shootLayer, float gunRange = default, Transform fireSocket = default)
    {
        Ray ray;
        if (fireSocket == default)
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

    protected virtual void ShootCapsuleCast(int damage, LayerMask shootLayer, float radius, float height)
    {
        Vector3 point1, point2;
        
        point1 = Camera.main.transform.position;
        point2 = point1 + Camera.main.transform.forward * height;                        

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
        float shootImpulse = Mathf.Lerp(this.shootImpulse, this.shootImpulse / 2f, distance / gunRange);
        hit.rigidbody.AddForce(-hit.normal * shootImpulse, ForceMode.Impulse);
    }

    private void OnEnable()
    {
        canFire = magAmmo > 0;
        
    }        
}

public interface ISecondaryAction
{
    void Perform();
}
