using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    [SerializeField] private float seconds = 5f;

    private void Start()
    {
        Destroy(gameObject, seconds);
    }
}
