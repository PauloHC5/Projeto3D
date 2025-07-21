using System;
using UnityEngine;

public class DamageColliderEvents : MonoBehaviour
{
    public event Action<Collider> onHitDamage;
    public event Action<Collider> onPersistanceDamage;
    
    private void OnTriggerEnter(Collider other)
    {
        onHitDamage?.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        onPersistanceDamage?.Invoke(other);
    }
}