using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : Gun, ISecondaryAction
{
    [SerializeField] private GameObject boltProjectile;
    [SerializeField] private Transform boltSpawnPoint;
    [SerializeField] private float boltForce = 100f;
    [SerializeField] private float scopeZoom = 30f;
    [SerializeField] private float scopeSpeed = 5f;    

    private bool wantsToAim = false;
    private float defaultFoV;
    private Coroutine aimCoroutine;

    private void Start()
    {
        Camera playerCamera = GameObject.FindFirstObjectByType<Camera>();
        if (playerCamera != null)
        {
            defaultFoV = playerCamera.fieldOfView;
        }
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
            yield return new WaitForSeconds(FireRate / 5);
        }        
    }

    protected override void FinishReload()
    {
        base.FinishReload();
    }


    public bool Perform()
    {
        wantsToAim = !wantsToAim;

        Camera[] playerCameras = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        if (playerCameras != null)
        {
            if (aimCoroutine != null)
            {
                StopCoroutine(aimCoroutine);
            }

            aimCoroutine = StartCoroutine(Aim(playerCameras));
        }
        else Debug.LogError("No camera found in the scene to scope zoom");

        return true;
    }


    private IEnumerator Aim(Camera[] playerCameras)
    {
        float elapsedTime = 0;
        float startFoV = playerCameras[0].fieldOfView;
        float targetFoV = wantsToAim ? scopeZoom : defaultFoV;        
        float localscopeSpeed = wantsToAim ? this.scopeSpeed : this.scopeSpeed * 3;        

        while (Mathf.Abs(playerCameras[0].fieldOfView - targetFoV) > 0.01f)
        {
            foreach (Camera playerCamera in playerCameras)
            {
                playerCamera.fieldOfView = Mathf.Lerp(startFoV, targetFoV, localscopeSpeed * (elapsedTime / scopeSpeed));
            }            
            elapsedTime += Time.deltaTime;            
            yield return null;
        }

        foreach (Camera playerCamera in playerCameras)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startFoV, targetFoV, localscopeSpeed * (elapsedTime / scopeSpeed));
        }
    }
}
