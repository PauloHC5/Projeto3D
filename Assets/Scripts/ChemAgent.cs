using UnityEngine;

public class ChemAgent : Enemy
{
    [Header("Chem Agent Properties")]
    [SerializeField] private ParticleSystem gunParticle;

    private readonly int Fire = Animator.StringToHash("Fire");

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat(Velocity, Mathf.Clamp(agent.velocity.sqrMagnitude, 0f, 1f));
        animator.SetBool(IsDead, isDead);

        if (animator.GetBool(Fire))
        {
            if (!gunParticle.isPlaying) gunParticle.Play();
            if (!SoundManager.instance.AmbienceSource.isPlaying && !PauseManager.Instance.IsPaused) SoundManager.PlaySoundInLoop(SoundType.CHEMAGENT, 0.5f);
        }
        else
        {
            if (gunParticle.isPlaying) gunParticle.Stop();
            if (SoundManager.instance.AmbienceSource.isPlaying) SoundManager.StopSoundInLoop(SoundType.CHEMAGENT);
        }
    }

    protected override void Die(WeaponTypes damageType)
    {
        gunParticle.Stop();
        animator.SetBool(Fire, false);

        base.Die(damageType);                
    }
}
