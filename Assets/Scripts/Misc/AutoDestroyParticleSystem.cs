using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour
{
    void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null && !ps.main.loop)
        {
            // Calculate total duration
            float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(gameObject, totalDuration);
        }
    }
}
