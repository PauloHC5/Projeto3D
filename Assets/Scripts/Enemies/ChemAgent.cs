using UnityEngine;

public class ChemAgent : Enemy    
{
    [Header("Chem Agent Properties")]
    [SerializeField] private ParticleSystem gunParticle;    
    [Header("Chem Agent Properties")] [SerializeField]
    private float distanceToStartFire = 5f;
    [SerializeField] private ParticleSystem gunParticle;
    [SerializeField] protected Collider damageCollider;

    private readonly int Fire = Animator.StringToHash("Fire");
    
    private void Awake()
    {
        OnAwake();

        // Set the distance to start firing in the behavior graph blackboard
        if (!behaviorGraph.BlackboardReference.SetVariableValue("DistanceToStartFire", distanceToStartFire))
        {
            Debug.LogWarning(
                "ChemAgent: Blackboard variable 'DistanceToStartFire' not found. Please ensure it is set in the Behavior Graph.");
        }
    }
    // Update is called once per frame
    void Update()
    {
        animator.SetFloat(Velocity, Mathf.Clamp(agent.velocity.sqrMagnitude, 0f, 1f));
        animator.SetBool(IsDead, isDead);

        if (animator.GetBool(Fire))
        {
            if (!gunParticle.isPlaying) gunParticle.Play();
            if(damageCollider) damageCollider.enabled = true;

            if(_audioSource) SoundManager.PlayRandomSFX(WorldSfxType.CHEMAGENT_GAS, _audioSource, true);
        }
        else
        {
            if (gunParticle.isPlaying) gunParticle.Stop();
            if (damageCollider) damageCollider.enabled = false;            
            if(_audioSource) _audioSource.Stop();
        }
    }

    protected override void Die(PlayerWeaponTypes damageType)
    {
        gunParticle.Stop();
        animator.SetBool(Fire, false);        

        base.Die(damageType);                
    }
}
