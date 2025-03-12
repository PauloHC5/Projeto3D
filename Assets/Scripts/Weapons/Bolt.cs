using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : MonoBehaviour
{
    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(DestroyAfterTime());
    }

    private void OnCollisionEnter(Collision collision)
    {        
        if (rb != null && collision.gameObject.isStatic)
        {
            rb.isKinematic = true;
        }        
    }

    // routine to destroy the bolt after 5 seconds
    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

}
