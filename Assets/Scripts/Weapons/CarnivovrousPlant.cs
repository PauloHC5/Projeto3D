using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarnivovrousPlant : Weapon
{
    [Header("Carnivovrous Plant Properties")]
    [SerializeField] private int _damage = 25;
    [SerializeField] private int _chewingDuration = 10;
    [SerializeField] private float _duration = 0.3f; // Duration for the scale-up effect

    private bool _canAttack = true;
    public bool CanAttack => _canAttack;

    private Collider _hitCollider;
    private Animator _animator;
    private Vector3 _originalScale;
    private AudioSource _audioSource;

    private int Chewing = Animator.StringToHash("Chewing");

    override protected float GetWeaponRange()
    {
        return 1.5f;
    }

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _hitCollider = GetComponent<Collider>();
        _originalScale = transform.localScale;
        _audioSource = GetComponent<AudioSource>();

        _audioSource.playOnAwake = false;
    }

    private void Update()
    {
        _animator.SetBool(Chewing, !CanAttack);
    }

    public void Attack()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("Attack");
            HUDManager.Bite();
            if(HUDManager.EnemyOnRange) StartCoroutine(AttackRoutine());
            SoundManager.PlayShootSound(_weaponType, _audioSource); // Play the attack sound
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
        _animator.SetTrigger(trigger);
    }

    public void EnableCollision()
    {
        _hitCollider.enabled = true;
    }

    public void DisableCollision()
    {
        _hitCollider.enabled = false;
    }    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().TakeDamage(_damage, _weaponType);
            _hitCollider.enabled = false; // Disable the collider after hitting
            
            StopCoroutine(ChewingRoutine());
            StartCoroutine(ChewingRoutine());
        }
    }    

    public override void DisableWeapon()
    {
        var childObjects = GetComponentsInChildren<Transform>(true);

        if (childObjects == null || childObjects.Length == 0)
        {
            Debug.LogWarning("No child objects found to disable.");
            return;
        }        

        foreach (Transform obj in childObjects)
        {
            if (obj.gameObject == gameObject) continue; // Skip the parent object itself
            obj.gameObject.SetActive(false); // Disable all child objects
        }
    }

    public override void EnableWeapon()
    {
        var childObjects = GetComponentsInChildren<Transform>(true);

        if (childObjects == null || childObjects.Length == 0)
        {
            Debug.LogWarning("No child objects found to enable.");
            return;
        }

        foreach (Transform obj in childObjects)
        {
            obj.gameObject.SetActive(true); // Enable all child objects
        }

        transform.localScale = _originalScale; // Reset scale when enabled
    }

    private IEnumerator AttackRoutine()
    {
        StartCoroutine(ScaleUpCoroutine(_originalScale * 2f)); // Scale up during attack
        yield return new WaitForSeconds(_duration); // Wait for the duration of the attack
        StartCoroutine(ScaleUpCoroutine(_originalScale)); // Scale back down after attack
    }

    private IEnumerator ScaleUpCoroutine(Vector3 scaleDesired)
    {
        float elapsed = 0f;
        while (elapsed < _duration)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, scaleDesired, elapsed / _duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = scaleDesired; // Ensure final scale is set
    }

    private IEnumerator ChewingRoutine()
    {
        _canAttack = false;        
        yield return new WaitForSeconds(_chewingDuration);
        _canAttack = true;        
    }
}
