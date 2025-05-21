using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private int health = 100;

    public int Health
    {
        get { return health; }
        set
        {
            health = value;
            if (health <= 0)
            {
                health = 0;

                //TODO: Handle player death                
                //Die();
            }
        }
    }
}
