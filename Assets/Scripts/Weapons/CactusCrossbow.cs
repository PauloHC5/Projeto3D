using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CactusCrossbow : ProjectileGun, ISecondaryAction
{
    [Header("Crossbow properties")]
    [SerializeField] private float intervalBetweenShots = 0.5f;    
    [SerializeField] private float scopeZoom = 30f;
    [SerializeField] private float scopeSpeed = 5f;    
    
    private Transform originalFireSocket;
    
    private CrossbowSpikes crossbowSpikes = new CrossbowSpikes();

    public static event Action<float, float> AimEvent;    

    private void Start()
    {        
        originalFireSocket = _fireSocket; // Store the original projectile spawn point        
    }

    private void Update()
    {
        crossbowSpikes.OnUpdate(gunAnimator, _magAmmo); // Update crossbow spikes animation based on ammo count        

        if (Camera.main.GetComponent<MouseLook>().ZoomIn)
            _fireSocket = Camera.main.transform;
        else
            _fireSocket = originalFireSocket; // Reset to original spawn point when not zoomed in        
    }

    public override void Fire()
    {               
        if(!_canFire || _magAmmo == 0) return;

        StartCoroutine(FireBurst());        
        _magAmmo -= 3;
        SoundManager.PlayShootSound(_weaponType, _gunAudioSource); // Play the attack sound
    }
    private IEnumerator FireBurst()
    {
        base.Fire();

        for (int i = 0; i < 2; i++)
        {
            base.ShootProjectile();
            yield return new WaitForSeconds(intervalBetweenShots);
        }        
    }            

    public void Perform()
    {                                
        AimEvent?.Invoke(scopeZoom, scopeSpeed);
    }        
}
