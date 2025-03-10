using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bolt : MonoBehaviour
{
    private Rigidbody rb;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.isStatic || (collision.rigidbody != null && collision.rigidbody.isKinematic))
        {
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }
}
