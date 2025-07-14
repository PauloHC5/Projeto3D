using UnityEngine;

public class DualPickupBehaviour : MonoBehaviour
{
    void Update()
    {
        // Destroy this GameObject if it has no children left
        if (transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
