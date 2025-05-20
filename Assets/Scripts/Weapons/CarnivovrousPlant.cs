using UnityEngine;

public class CarnivovrousPlant : Weapon
{
    [Header("Carnivovrous Plant Properties")]
    [SerializeField] private int damage = 25;
    [SerializeField] private Collider hitCollider;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        else
        {
            Debug.LogWarning("Animator not found on Carnivorous Plant.");
        }
    }

    public void PlayRaise(string trigger)
    {
        animator.SetTrigger(trigger);
    }

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
