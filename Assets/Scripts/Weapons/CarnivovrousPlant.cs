using UnityEngine;

public class CarnivovrousPlant : Weapon
{
    [Header("Carnivovrous Plant Properties")]
    [SerializeField] private int damage = 25;
    [SerializeField] private Collider hitCollider;

    public void EnableCollision()
    {
        hitCollider.enabled = true;
    }

    public void DisableCollision()
    {
        hitCollider.enabled = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(damage, weaponType);
        }
    }
}
