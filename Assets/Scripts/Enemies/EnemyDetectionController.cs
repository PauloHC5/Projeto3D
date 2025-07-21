using System;
using UnityEngine;

public class EnemyDetectionController : MonoBehaviour
{
    [Header("Line of Sight Detector Properties")]
    [SerializeField] private float detectionRange = 10.0f;
    [SerializeField] private float detectionHeight = 3.0f;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private bool targetDetected = false;
    
    private GameObject target;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        targetDetected = PerformLineOfSightDetection(target);
    }

    public bool PerformLineOfSightDetection(GameObject potentialTarget)
    {
        RaycastHit hit;
        Vector3 direction = potentialTarget.transform.position - raycastOrigin.position;
        direction.y += detectionHeight; // Adjust the direction to include detectionHeight

        // Project a raycast
        if (Physics.Raycast(raycastOrigin.position, direction, out hit, detectionRange, detectionLayer))
        {
            if (hit.collider.gameObject == potentialTarget)
            {
                Debug.DrawRay(raycastOrigin.position, direction * detectionRange, Color.red);
                return true; // Target detected within the detection range
            }
            else
            {
                if(showDebugVisuals && this.enabled) Debug.DrawRay(raycastOrigin.position, direction * detectionRange, Color.green);
            }
        }

        return false;
    }
}
