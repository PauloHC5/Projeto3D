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

    [Header("Gun Components")]
    [SerializeField] protected Transform fireSocket;
    [SerializeField] protected ParticleSystem muzzleFlash;
    [SerializeField] protected ParticleSystem impactVFX;
    [SerializeField] protected bool DebugRaycast = false;

    private Int32 ammoToReload = 0;
    public Int32 AmmoToReload { get { return ammoToReload; } set { ammoToReload = value; } }

    public float FireRate { get { return fireRate; } }

    private bool canFire = true;
    public bool CanFire { get { return canFire; } }

    // Getter and setter for magAmmo
    public int MagAmmo { get => magAmmo; }

    // Getter for maxAmmo
    public int MaxAmmo => maxAmmo;

    // Animator properties
    private Animator gunAnimator;
    private readonly int FireTrigger = Animator.StringToHash("Fire");
    private readonly int ReloadTrigger = Animator.StringToHash("Reload");    

    private void Awake()
    {
        gunAnimator = GetComponent<Animator>();
        if (gunAnimator == null) gunAnimator = GetComponentInChildren<Animator>();

        magAmmo = magAmmo > maxAmmo ? magAmmo = maxAmmo : magAmmo;
    }    

    public virtual void Fire()
    {
        if (muzzleFlash) muzzleFlash.Play();
        if (gunAnimator) gunAnimator.SetTrigger(FireTrigger);
        StartCoroutine(ShootDelay());
    }

    public virtual void PlayReload()
    {
        gunAnimator.SetTrigger(ReloadTrigger);
    }

    public virtual void Reload()
    {
        var weaponAmmo = GameManager.Instance.Player.WeaponAmmo[GameManager.Instance.Player.WeaponSelected];

        // Get the difference between the max ammo and the current ammo
        int ammoDifference = MaxAmmo - MagAmmo;

        // Get the ammo to reload
        ammoToReload = weaponAmmo >= ammoDifference ? ammoDifference : weaponAmmo;

        // Subtract the ammo to reload from the weapon ammo
        GameManager.Instance.Player.WeaponAmmo[GameManager.Instance.Player.WeaponSelected] -= ammoToReload;

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

    protected virtual void ShootRaycast(int damage, LayerMask shootLayer, float gunRange = default)
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

    private void OnEnable()
    {
        canFire = magAmmo > 0;
        PlayerCharacterCombatController.onSwitchToWeapon += OnSwitchToWeapon;
    }

    protected virtual void OnSwitchToWeapon(PlayerWeapon weapon)
    {
        // To be implemented in derived classes
    }

    private void OnDisable()
    {
        PlayerCharacterCombatController.onSwitchToWeapon -= OnSwitchToWeapon;
    }
}

public interface ISecondaryAction
{
    void Perform();
}
