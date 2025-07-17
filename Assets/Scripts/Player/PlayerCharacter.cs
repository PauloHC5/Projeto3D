using System.Collections;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private float regenerationWaitTime = 5f; // Health regeneration rate per second
    [SerializeField] private float regenerationRate = 1f; // Health regeneration rate per second

    private Coroutine _healthRegenerationCoroutine;

    public int Health
    {
        get { return health; }
        set
        {
            if (!enabled) return;

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

    private void Die()
    {
        if (!enabled) return;

        Debug.Log("Player has died.");                        
        GameManager.GameOver();

        Camera.main.transform.SetParent(null); // Unparent the camera from the player character

        Destroy(gameObject); // Destroy the player character object
    }

    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(regenerationWaitTime); // Wait before starting regeneration
        while (health < 100) // Assuming 100 is the maximum health
        {
            health += Mathf.RoundToInt(regenerationRate);
            health = Mathf.Min(health, 100); // Ensure health does not exceed maximum
            yield return new WaitForSeconds(1f); // Wait for 1 second before next regeneration
        }
    }
}
