using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected int damage = 10;
    protected Rigidbody rb;    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(DestroyAfterTime());
    }

    protected IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Woodsman enemy = collision.gameObject.GetComponent<Woodsman>();
        if (enemy)
        {
            enemy.TakeDamage(damage, WeaponTypes.Pistol);
            Destroy(gameObject);
        }
    }

}
