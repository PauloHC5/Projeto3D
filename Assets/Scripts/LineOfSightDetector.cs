using UnityEngine;

public class LineOfSightDetector : MonoBehaviour
{
    [SerializeField] private float detectionRange = 10.0f;
    [SerializeField] private float detectionHeight = 3.0f;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private bool showDebugVisuals = true;
    [SerializeField] private LayerMask playerMask;

    public GameObject PerformDetection(GameObject potentialTarget)
    {            
        RaycastHit hit;
        Vector3 direction = potentialTarget.transform.position - raycastOrigin.position;
        direction.y += detectionHeight; // Adjust the direction to include detectionHeight

        // Project a raycast
        if (Physics.Raycast(raycastOrigin.position, direction, out hit, detectionRange, playerMask))
        {
            if (showDebugVisuals && this.enabled)
            {
                Debug.DrawRay(raycastOrigin.position, direction * detectionRange, Color.red);                
            }

            if (hit.collider.gameObject == potentialTarget)
            {                        
                return hit.collider.gameObject;
            }
        }
        else
        {
            if (showDebugVisuals && this.enabled)
            {
                Debug.DrawRay(raycastOrigin.position, direction * detectionRange, Color.green);                
            }
        }

        return null; 
    }
}
