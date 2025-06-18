using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : Weapon, IEquippedGun
{

    [Header("Gun Properties")]
    [SerializeField] protected float _fireRate = 0.5f;    
    [SerializeField] protected int _magCapacity = 40;
    [SerializeField] protected int _magAmmo = 40;
    [SerializeField] protected AmmoTypes _ammoType;    

    [Header("Recoil Properties")]
    [SerializeField] protected float _recoilX = -2f;
    [SerializeField] protected float _recoilY = 2f;
    [SerializeField] protected float _recoilZ = 0.35f;
    [SerializeField] protected float _snappiness = 6f;
    [SerializeField] protected float _returnSpeed = 2f;

    [Header("Gun Components")]
    [SerializeField] protected Transform _fireSocket;
    [SerializeField] protected ParticleSystem _muzzleFlash;    
    
    protected bool _canFire = true;
    protected AudioSource _gunAudioSource; // Audio source for gun sounds
    private CameraRecoil _cameraRecoil;

    // Animator properties
    protected Animator gunAnimator;
    private readonly int FireTrigger = Animator.StringToHash("Fire");
    private readonly int ReloadTrigger = Animator.StringToHash("Reload");

    // Getters and Setters    
    public float FireRate { get { return _fireRate; } }            
    public int MagAmmo { get => _magAmmo;
        set
        {
           _magAmmo = Mathf.Clamp(value, 0, _magCapacity); // Ensure magAmmo does not exceed magCapacity or go below 0                                                                 
           _canFire = _magAmmo > 0; // Update canFire based on magAmmo
        }   
    }
    
    public int MagCapacity => _magCapacity;

    public bool CanFire => _canFire;

    public bool CanReload() => _magAmmo < _magCapacity;

    public AmmoTypes AmmoType => _ammoType;

    protected virtual void Awake()
    {        
        gunAnimator = GetComponent<Animator>();
        if (gunAnimator == null) gunAnimator = GetComponentInChildren<Animator>();
        
        _magAmmo = _magAmmo > _magCapacity ? _magAmmo = _magCapacity : _magAmmo; // Clamp magAmmo to maxAmmo                

        _cameraRecoil = Camera.main.GetComponentInParent<CameraRecoil>(); 
        
        _gunAudioSource = GetComponent<AudioSource>();
        _gunAudioSource.playOnAwake = false;
    }

    public virtual void Fire()
    {
        if (!_canFire || _magAmmo == 0) return;

        if (gunAnimator) gunAnimator.SetTrigger(FireTrigger);
        else Debug.LogWarning("Gun animator not found.");

        if (_muzzleFlash) _muzzleFlash.Play();        
        if (_cameraRecoil) _cameraRecoil.RecoilFire(_recoilX, _recoilY, _recoilZ, _snappiness, _returnSpeed);        

        StartCoroutine(ShootDelay());
    }

    public void DoubleRecoil()
        {
        if (_cameraRecoil)
        {
            _cameraRecoil.RecoilFire(_recoilX * 2, _recoilY * 2, _recoilZ * 2, _snappiness, _returnSpeed);
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

        _canFire = _magAmmo > 0;
    }

    protected IEnumerator ShootDelay()
    {
        _canFire = false;        
        yield return new WaitForSeconds(FireRate);
        _canFire = true;        
    }

    private void OnEnable()
    {
        _canFire = _magAmmo > 0; // Reset canFire when the gun is enabled
    }
}

public interface ISecondaryAction
{
    void Perform();
}
