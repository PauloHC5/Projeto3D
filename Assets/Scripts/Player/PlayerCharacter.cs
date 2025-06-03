using System.Collections;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private float regenerationWaitTime = 5f; // Health regeneration rate per second
    [SerializeField] private float regenerationRate = 1f; // Health regeneration rate per second

    private Coroutine healthRegenerationCoroutine;

    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if (healthRegenerationCoroutine != null)
            {
                StopCoroutine(healthRegenerationCoroutine); // Stop any existing regeneration coroutine
            }
            healthRegenerationCoroutine = StartCoroutine(RegenerateHealth()); // Start a new regeneration coroutine


            if (health <= 0)
            {
                health = 0;                
                
                Die();
            }
        }
    }

    private void Die()
    {
        Debug.Log("Player has died.");
        // Implement player death logic here, such as playing a death animation, showing a game over screen, etc.
        // For example:
        // animator.SetTrigger("Die");
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
