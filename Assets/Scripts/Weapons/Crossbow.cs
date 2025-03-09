using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossbow : Gun, ISecondaryAction
{
    [SerializeField] private float scopeZoom = 30f;
    [SerializeField] private float scopeSpeed = 5f;

    private bool WantsToAim = false;    

    public bool Perform()
    {
        WantsToAim = !WantsToAim;        
        StopAllCoroutines();
        
        Camera playerCamera = GameObject.FindFirstObjectByType<Camera>();

        StartCoroutine(Aim(playerCamera));        
        return true;
    }


    private IEnumerator Aim(Camera playerCamera)
    {               
        float elapsedTime = 0;        

        if(WantsToAim)
        {
            float startFoF = playerCamera.fieldOfView;
            float desiredFoV = scopeZoom;
            while (playerCamera.fieldOfView > desiredFoV)
            {
                playerCamera.fieldOfView = Mathf.Lerp(startFoF, desiredFoV, scopeSpeed * (elapsedTime / 90f));
                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }
        else
        {
            float startFoF = playerCamera.fieldOfView;
            float desiredFoV = 90f; // TODO: Make this var get the default FoV from the player camera configs

            while (playerCamera.fieldOfView < desiredFoV)
            {
                playerCamera.fieldOfView = Mathf.Lerp(startFoF, desiredFoV, scopeSpeed * (elapsedTime / 40f));
                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }        
        
        yield return null;
    }
}
