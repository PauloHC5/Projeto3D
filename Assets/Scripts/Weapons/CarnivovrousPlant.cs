using System.Collections;
using UnityEngine;

public class CarnivovrousPlant : Weapon
{
    [Header("Carnivovrous Plant Properties")]
    [SerializeField] private int damage = 25;
    [SerializeField] private int chewingDuration = 10;
    [SerializeField] private float duration = 0.3f; // Duration for the scale-up effect

    private bool canAttack = true;
    public bool CanAttack => canAttack;

    private Collider hitCollider;
    private Animator animator;
    private Vector3 originalScale;

    private int Chewing = Animator.StringToHash("Chewing");

    override protected float GetWeaponRange()
    {
        return 1.5f;
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        hitCollider = GetComponent<Collider>();
        originalScale = transform.localScale;        
    }

    public void Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
            GameManager.Instance?.Hud?.Bite();
            if(GameManager.Instance.Hud.EnemyOnRange) StartCoroutine(AttackRoutine());
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().TakeDamage(damage, weaponType);
            hitCollider.enabled = false; // Disable the collider after hitting
            
            StopCoroutine(ChewingRoutine());
            StartCoroutine(ChewingRoutine());
        }
    }

    private void OnEnable()
    {
        transform.localScale = originalScale; // Reset scale when enabled
    }

    private IEnumerator AttackRoutine()
    {
        StartCoroutine(ScaleUpCoroutine(originalScale * 2f)); // Scale up during attack
        yield return new WaitForSeconds(duration); // Wait for the duration of the attack
        StartCoroutine(ScaleUpCoroutine(originalScale)); // Scale back down after attack
    }

    private IEnumerator ScaleUpCoroutine(Vector3 scaleDesired)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scaleDesired, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = scaleDesired; // Ensure final scale is set
    }

    private IEnumerator ChewingRoutine()
    {
        canAttack = false;
        animator.SetBool(Chewing, true);
        yield return new WaitForSeconds(chewingDuration);
        canAttack = true;
        animator.SetBool(Chewing, false);
    }
}
