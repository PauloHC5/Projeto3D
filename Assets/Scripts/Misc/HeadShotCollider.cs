using System;
using UnityEngine;

public class HeadShotCollider : MonoBehaviour
{
    private Enemy _enemy;

    private void Awake()
    {
        _enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var spike = other.GetComponent<SpikeProjectile>();
        
        if (spike == null) return;
        
        _enemy?.TakeDamage(spike.Damage * 2, DamageType.Spike);
        Destroy(other.gameObject);
    }
}
