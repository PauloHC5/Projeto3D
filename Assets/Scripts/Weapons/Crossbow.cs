using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : Gun, ISecondaryAction
{
    [Header("Crossbow properties")]
    [SerializeField] private float intervalBetweenShots = 0.5f;
    [SerializeField] private GameObject boltProjectile;
    [SerializeField] private Transform boltSpawnPoint;
    [SerializeField] private float boltForce = 100f;
    [SerializeField] private float scopeZoom = 30f;
    [SerializeField] private float scopeSpeed = 5f;    
    
    public static event Action<float, float> AimEvent;    

    private void Start()
    {
        Camera playerCamera = GameObject.FindFirstObjectByType<Camera>();        
    }

    public override void Fire()
    {
        //base.ShootProjectile(boltProjectile, boltSpawnPoint, boltForce);
        StartCoroutine(FireBurst());
        base.Fire();
        magAmmo -= 3;
    }
    private IEnumerator FireBurst()
    {
        for (int i = 0; i < 3; i++)
        {
            base.ShootProjectile(boltProjectile, boltSpawnPoint, boltForce);
            yield return new WaitForSeconds(intervalBetweenShots);
        }        
    }    

    public void Perform()
    {                        
        AimEvent?.Invoke(scopeZoom, scopeSpeed);
    }            
}
