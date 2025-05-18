using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : Gun, ISecondaryAction
{
    [Header("Crossbow properties")]
    [SerializeField] private float intervalBetweenShots = 0.5f;
    [SerializeField] private GameObject boltProjectile;    
    [SerializeField] private float boltForce = 100f;
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
        StartCoroutine(FireBurst());
        base.Fire();
        magAmmo -= 3;
    }
    private IEnumerator FireBurst()
    {
        for (int i = 0; i < 3; i++)
        {
            base.ShootProjectile(boltProjectile, fireSocket, boltForce);
            yield return new WaitForSeconds(intervalBetweenShots);
        }        
    }            

    public void Perform()
    {                        
        AimEvent?.Invoke(scopeZoom, scopeSpeed);
    }

    public void OnFinishReload()
    {
        Reload();
    }

    private void OnEnable()
    {
        AnimationTriggerEvents.onFinishReload += OnFinishReload;
    }

    private void OnDisable()
    {
        AnimationTriggerEvents.onFinishReload -= OnFinishReload;
    }
}
