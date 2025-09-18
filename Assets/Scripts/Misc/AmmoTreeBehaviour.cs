using System.Collections;
using UnityEngine;

public class AmmoTreeBehaviour : MonoBehaviour
{
    [SerializeField] private AmmoTypes ammoType;
    [SerializeField] private MeshRenderer meshWithAmmo;
    [SerializeField] private MeshRenderer meshWithoutAmmo;
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private float timeBetweenAmmoRespawn = 30f;
    [SerializeField] private int ammoAmount = 10;
    
    private bool hasAmmo = true;
    
    public AmmoTypes AmmoType => ammoType;
    
    private void Start()
    {
        if (meshWithAmmo == null || meshWithoutAmmo == null)
        {
            Debug.LogError("Mesh renderers are not assigned in AmmoTreeBehaviour.");
            return;
        }
        
        meshWithAmmo.enabled = true;
        meshWithoutAmmo.enabled = false;
    }
    
    public int CollectAmmo() 
    {
        triggerCollider.enabled = false;
        hasAmmo = false;
        meshWithAmmo.enabled = false;
        meshWithoutAmmo.enabled = true;
        
        StartCoroutine(nameof(RegenAmmoAfterDelay));
        
        return ammoAmount;
    }   
    
    private IEnumerator RegenAmmoAfterDelay()
    {
        yield return new WaitForSeconds(timeBetweenAmmoRespawn);
        
        hasAmmo = true;
        meshWithAmmo.enabled = true;
        meshWithoutAmmo.enabled = false;
        triggerCollider.enabled = true;
    }
}
