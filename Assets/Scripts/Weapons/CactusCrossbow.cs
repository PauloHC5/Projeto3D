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
    
    private CrossbowSpikes crossbowSpikes = new CrossbowSpikes();

    public static event Action<float, float> AimEvent;    

    private void Start()
    {
        Camera playerCamera = GameObject.FindFirstObjectByType<Camera>();        
    }

    private void Update()
    {
        crossbowSpikes.OnUpdate(gunAnimator, magAmmo); // Update crossbow spikes animation based on ammo count
    }

    public override void Fire()
    {               
        if(!canFire) return; // Prevent firing if cannot fire

        StartCoroutine(FireBurst());        
        magAmmo -= 3;
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
