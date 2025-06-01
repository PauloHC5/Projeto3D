using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BananaShotgun : RaycastGun    
{
    [Header("Shotgun Properties")]        
    [SerializeField] private float shootRadius = 0.5f;
    [SerializeField] private float throwForce = 10f;    

    [Header("Burst VFX Properties")]
    [SerializeField] private float burstRange = 10f;
    [SerializeField] private int pelletsPerShot = 5;
    [SerializeField] private float spreadAngle = 5f;

    private Rigidbody rb;
    private Collider shotgunCollider;

    private readonly int magAmmoHash = Animator.StringToHash("MagAmmo");

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody>();
        shotgunCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        gunAnimator.SetInteger(magAmmoHash, magAmmo);
    }

    public override void Fire()
    {        
        base.ShootCapsuleCast(shootRadius);
        base.Fire();
        StartCoroutine(BurstFire());
        magAmmo--;
        SoundManager.PlayShootSound(weaponType, 0.5f); // Play the attack sound
    }

    public override void PerformReload()
    {
        base.PerformReload();
    }


    public void DropShotgun()
    {
        // Desaatch shotgun from player
        transform.SetParent(null);

        // Set shotgun to default layer
        int defaultLayer = LayerMask.NameToLayer("Default");
        gameObject.layer = defaultLayer;

        // Find all objectss in this prefab and set their layer to default        
        foreach (Transform children in gameObject.GetComponentsInChildren<Transform>())
        {
            children.gameObject.layer = defaultLayer;
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        shotgunCollider.enabled = true;
        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        AnimationTriggerEvents.onDropShotgun -= DropShotgun;
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < pelletsPerShot; i++)
        {
            BurstRaycast(shootLayer, burstRange);     
            yield return new WaitForEndOfFrame();
        }        
    }    

    private void BurstRaycast(LayerMask shootLayer, float burstRange)
    {
        Vector3 direction = fireSocket.forward;
        direction.x += Random.Range(-spreadAngle, spreadAngle);
        direction.y += Random.Range(-spreadAngle, spreadAngle);        

        RaycastHit hit;
        if (Physics.Raycast(fireSocket.position, direction, out hit, burstRange, shootLayer))
        {            
            if (impactVFX != null)
            {
                Instantiate(impactVFX, hit.point, Quaternion.LookRotation(hit.normal));
            }

            // Check if the object hit has rigidbody
            if (hit.rigidbody && !hit.collider.CompareTag("Enemy"))
            {
                ApplyImpulse(hit);
            }
        }

        if (DebugRaycast)
        {
            Debug.DrawRay(fireSocket.position, direction * burstRange, Color.red, 5f);
        }        
    }

    private void OnDrawGizmos()
    {
        if (DebugRaycast)
        {
            Vector3 point1, point2;            

            point1 = Camera.main.transform.position;
            point2 = point1 + Camera.main.transform.forward * gunRange;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point1, shootRadius);
            Gizmos.DrawWireSphere(point2, shootRadius);
            Gizmos.DrawLine(point1 + Vector3.up * shootRadius, point2 + Vector3.up * shootRadius);
            Gizmos.DrawLine(point1 - Vector3.up * shootRadius, point2 - Vector3.up * shootRadius);
            Gizmos.DrawLine(point1 + Vector3.right * shootRadius, point2 + Vector3.right * shootRadius);
            Gizmos.DrawLine(point1 - Vector3.right * shootRadius, point2 - Vector3.right * shootRadius);
        }
    }

    private void OnEnable()
    {
        AnimationTriggerEvents.onDropShotgun += DropShotgun;
    }

    private void OnDisable()
    {
        AnimationTriggerEvents.onDropShotgun -= DropShotgun;
    }
}
