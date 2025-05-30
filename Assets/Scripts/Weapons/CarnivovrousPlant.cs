using UnityEngine;

public class CarnivovrousPlant : Weapon
{
    [Header("Carnivovrous Plant Properties")]
    [SerializeField] private int damage = 25;
    
    
    private Collider hitCollider;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        hitCollider = GetComponent<Collider>();
    }

    public void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            GameManager.Instance?.Hud?.Bite();
        }
        else
        {
            Debug.LogWarning("Animator not found on Carnivorous Plant.");
        }

        PlayerCharacterCombatController playerCombat = GetComponentInParent<PlayerCharacterCombatController>();
        if (playerCombat != null) playerCombat.PlayerCharacterAnimationsController.PlayUseWeapon();
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
            Debug.Log("Hit enemy with carnivorous plant");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().TakeDamage(damage, weaponType);
            hitCollider.enabled = false; // Disable the collider after hitting
            Debug.Log("Hit enemy with carnivorous plant");
        }
    }
}
