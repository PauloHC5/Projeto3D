using System;
using System.Collections;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [Header("Health Properties")]
    [SerializeField] private int health = 100;
    [SerializeField] private float regenerationWaitTime = 5f; // Health regeneration rate per second
    [SerializeField] private float regenerationRate = 1f; // Health regeneration rate per second

    [Header("Death Properties")] 
    [SerializeField] private GameObject playerDeathObject;
    [SerializeField] private GameObject playerArmsMesh;
    
    private PlayerCharacterController _playerCharacterController;
    private PlayerCharacterMovementController _playerCharacterMovementController;
    private PlayerCharacterCombatController _playerCharacterCombatController;
    private PlayerCharacterAnimationsController _playerCharacterAnimationsController;
    private CharacterController _characterController;
    private AudioSource _audioSource;
    
    private Coroutine _healthRegenerationCoroutine;
    private Coroutine _stopCoughingCoroutine;
    
    public Action OnDeath;
    public Action OnRegeneration;

    private void Awake()
    {
        _playerCharacterController = GetComponent<PlayerCharacterController>();
        _playerCharacterMovementController = GetComponent<PlayerCharacterMovementController>();
        _playerCharacterCombatController = GetComponent<PlayerCharacterCombatController>();
        _characterController = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        
        _playerCharacterAnimationsController = new PlayerCharacterAnimationsController(GetComponentInChildren<Animator>());
    }

    public int Health
    {
        get { return health; }
        
        private set
        {
            health = value;
            if (_healthRegenerationCoroutine != null)
            {
                StopCoroutine(_healthRegenerationCoroutine); // Stop any existing regeneration coroutine
            }
            _healthRegenerationCoroutine = StartCoroutine(RegenerateHealth()); // Start a new regeneration coroutine

            if (health <= 0)
            {
                health = 0;                
                Die();
            }
        }
    }

    public void TakeDamage(int damage, DamageType damageType)
    {
        if (!enabled) return;

        switch (damageType)
        {
            case DamageType.Axe:
                SoundManager.PlayRandomSFX(GlobalSfxTypes.HIT);
                HUDManager.ShowBloodScreen();
                break;
            case DamageType.Gas:
                SoundManager.PlayRandomSFX(WorldSfxType.COUGHING, _audioSource, true);
                if (_stopCoughingCoroutine != null)
                {
                    StopCoroutine(_stopCoughingCoroutine);
                }
                _stopCoughingCoroutine = StartCoroutine(StopCoughtingAfterDelay());
                
                HUDManager.ShowGasPoisoningScreen();
                break;
        }
        
        Health -= damage;
    }

    private void Die()
    {
        if (!enabled) return;

        Debug.Log("Player has died.");                        
        GameManager.GameOver();
        
        _audioSource.Stop();

        playerDeathObject.transform.rotation = Camera.main.transform.rotation;
        Camera.main.transform.SetParent(playerDeathObject.transform);
        playerArmsMesh.transform.SetParent(playerDeathObject.transform);
        _playerCharacterAnimationsController.PlayDeath();
        _playerCharacterMovementController.enabled = false;
        _playerCharacterCombatController.EquippedWeapon.DropWeapon();
        _playerCharacterCombatController.enabled = false;
        _characterController.enabled = false;
        _playerCharacterController.enabled = false;
        
        playerDeathObject.SetActive(true);
        
        OnDeath?.Invoke();
        
        enabled = false;
    }

    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(regenerationWaitTime); // Wait before starting regeneration
        while (health < 100 && health != 0) // Assuming 100 is the maximum health
        {
            health += Mathf.RoundToInt(regenerationRate);
            health = Mathf.Min(health, 100); // Ensure health does not exceed maximum
            OnRegeneration?.Invoke();
            yield return new WaitForSeconds(1f); // Wait for 1 second before next regeneration
        }
    }
    
    private IEnumerator StopCoughtingAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
}
