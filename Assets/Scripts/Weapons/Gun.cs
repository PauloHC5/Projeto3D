using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon, IEquippedGun
{

    [Header("Gun Properties")]
    [SerializeField] protected float fireRate = 0.5f;    
    [SerializeField] protected int magCapacity = 40;
    [SerializeField] protected int magAmmo = 40;
    [SerializeField] protected AmmoTypes ammoType;

    [Header("Recoil Properties")]
    [SerializeField] protected float recoilX = -2f;
    [SerializeField] protected float recoilY = 2f;
    [SerializeField] protected float recoilZ = 0.35f;
    [SerializeField] protected float snappiness = 6f;
    [SerializeField] protected float returnSpeed = 2f;

    [Header("Gun Components")]
    [SerializeField] protected Transform fireSocket;
    [SerializeField] protected ParticleSystem muzzleFlash;    
    
    protected bool canFire = true;    
    private CameraRecoil cameraRecoil;

    // Animator properties
    protected Animator gunAnimator;
    private readonly int FireTrigger = Animator.StringToHash("Fire");
    private readonly int ReloadTrigger = Animator.StringToHash("Reload");

    // Getters and Setters    
    public float FireRate { get { return fireRate; } }            
    public int MagAmmo { get => magAmmo;
        set
        {
           magAmmo = Mathf.Clamp(value, 0, magCapacity); // Ensure magAmmo does not exceed magCapacity or go below 0                                                                 
           canFire = magAmmo > 0; // Update canFire based on magAmmo
        }   
    }
    
    public int MagCapacity => magCapacity;

    public bool CanFire => canFire;

    public bool CanReload() => magAmmo < magCapacity;

    public AmmoTypes AmmoType => ammoType;

    protected virtual void Awake()
    {        
        gunAnimator = GetComponent<Animator>();
        if (gunAnimator == null) gunAnimator = GetComponentInChildren<Animator>();
        
        magAmmo = magAmmo > magCapacity ? magAmmo = magCapacity : magAmmo; // Clamp magAmmo to maxAmmo                

        cameraRecoil = Camera.main.GetComponentInParent<CameraRecoil>();             
    }

    public virtual void Fire()
    {
        if (!canFire || magAmmo == 0) return;

        if (gunAnimator) gunAnimator.SetTrigger(FireTrigger);
        else Debug.LogWarning("Gun animator not found.");

        if (muzzleFlash) muzzleFlash.Play();        
        if (cameraRecoil) cameraRecoil.RecoilFire(recoilX, recoilY, recoilZ, snappiness, returnSpeed);        

        StartCoroutine(ShootDelay());
    }

    public void DoubleRecoil()
        {
        if (cameraRecoil)
        {
            cameraRecoil.RecoilFire(recoilX * 2, recoilY * 2, recoilZ * 2, snappiness, returnSpeed);
        }        
    }

    public virtual void PerformReload()
    {
        if(!CanReload())
        {
            Debug.Log("Magazine is already full.");
            return;
        }

        if (gunAnimator) gunAnimator.SetTrigger(ReloadTrigger);
        else Debug.LogWarning("Gun animator not found.");        
    }

    public virtual void Reload(ref int playerGunAmmo)
    {        
        int ammoAmountToReload = MagCapacity - MagAmmo; // Calculate the ammo to reload
        if (playerGunAmmo < ammoAmountToReload) // If the ammo to reload is greater than the player ammo
        {
            ammoAmountToReload = playerGunAmmo; // Set the ammo to reload to the player ammo
        }

        MagAmmo += ammoAmountToReload; // Set the mag ammo to the ammo to reload
        playerGunAmmo -= ammoAmountToReload; // Subtract the ammo from the player ammo

        canFire = magAmmo > 0;
    }

    protected IEnumerator ShootDelay()
    {
        canFire = false;        
        yield return new WaitForSeconds(FireRate);
        canFire = true;        
    }

    private void OnEnable()
    {
        canFire = magAmmo > 0; // Reset canFire when the gun is enabled
    }
}

public interface ISecondaryAction
{
    void Perform();
}
