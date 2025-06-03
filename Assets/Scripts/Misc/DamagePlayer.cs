
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    [SerializeField] Enemy enemy;
    [SerializeField] private bool isTriggerEnter = true;

    private void OnTriggerStay(Collider other)
    {
        if (isTriggerEnter) return;
        // Check if the collider belongs to a player character
        PlayerCharacter player = other.GetComponent<PlayerCharacter>();        

        if (player)
        {
            player.Health -= enemy.Damage;
        }        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggerEnter) return;
        // Check if the collider belongs to a player character
        PlayerCharacter player = other.GetComponent<PlayerCharacter>();
        if (player)
        {
            player.Health -= enemy.Damage;
        }
    }
}
