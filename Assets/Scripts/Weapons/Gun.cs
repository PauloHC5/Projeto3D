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

    [Header("Recoil Properties")]
    [SerializeField] private float recoilX = -2f;
    [SerializeField] private float recoilY = 2f;
    [SerializeField] private float recoilZ = 0.35f;
    [SerializeField] private float snappiness = 6f;
    [SerializeField] private float returnSpeed = 2f;

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
           canFire = magAmmo > 0;                                                         
        }   
    }
    
    public int MagCapacity => magCapacity;

    public bool CanFire => canFire;

    public bool CanReload() => magAmmo < magCapacity;


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

    protected IEnumerator ShootDelay()
    {
        canFire = false;
        Debug.Log(canFire);
        yield return new WaitForSeconds(FireRate);
        canFire = true;
        Debug.Log(canFire);
    }      
}

public interface ISecondaryAction
{
    void Perform();
}
