using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class EnemyAnimationsControlller
    {
        protected readonly Animator _enemyAnimator;
        protected readonly NavMeshAgent _enemyAgent;
        
        private const int ReactionLayerIndex = 1; // Index of the reaction layer in the animator
        private const float MediumLayerWeight = 0.75f; // Medium layer weight for the reaction layer
        private const float FullLayerWeight = 1.0f; // Full layer weight for the reaction layer 
        
        private readonly int _isDead = Animator.StringToHash("IsDead");
        private readonly int _velocity = Animator.StringToHash("Velocity");
        private readonly int _acornReact = Animator.StringToHash("ACornReact");
        private readonly int _shotgunReact = Animator.StringToHash("ShotgunReact");
        private readonly int _crossbowReact = Animator.StringToHash("CrossbowReact");
        private readonly int _acornDeath = Animator.StringToHash("ACornDeath");
        private readonly int _shotgunDeath = Animator.StringToHash("ShotgunDeath");
        private readonly int _crossbowDeath = Animator.StringToHash("CrossbowDeath");
        private readonly int _shotgunStun = Animator.StringToHash("ShotgunStun");
        private readonly int _superAcornReact = Animator.StringToHash("SuperACornReact");
        private readonly int _superAcornDeath = Animator.StringToHash("SuperACornDeath");
        
        
        public Animator Animator => _enemyAnimator;
        
        public EnemyAnimationsControlller(Animator enemyAnimator, NavMeshAgent enemyAgent)
        {
            _enemyAnimator = enemyAnimator;
            _enemyAgent = enemyAgent;
        }
        
        public void HandleLocomotion()
        {
            if (_enemyAnimator == null) return;
            
            _enemyAnimator.SetFloat(_velocity, _enemyAgent.velocity.sqrMagnitude);
        }

        public void PlayTakeDamage(DamageType damageType)
        {
            float layerWeight = (damageType == DamageType.Spike) ? MediumLayerWeight : FullLayerWeight;
            _enemyAnimator.SetLayerWeight(ReactionLayerIndex, layerWeight);

            switch (damageType)
            {
                case DamageType.Acorn:
                    _enemyAnimator.SetTrigger(_acornReact);
                    break;
                case DamageType.Banana:
                    _enemyAnimator.SetTrigger(_shotgunReact);
                    break;
                case DamageType.Spike:
                    _enemyAnimator.SetTrigger(_crossbowReact);
                    break;
                case DamageType.SuperAcorn:
                    _enemyAnimator.SetTrigger(_superAcornReact);
                    break;
            }
        }

        public virtual void PlayStun()
        {
            _enemyAnimator.SetTrigger(_shotgunStun);
        }
        
        public void PlayDeath(DamageType damageType)
        {
            _enemyAnimator.SetBool(_isDead, true);

            switch (damageType)
            {
                case DamageType.Acorn:
                    _enemyAnimator.SetTrigger(_acornDeath);
                    break;
                case DamageType.Banana:
                    _enemyAnimator.SetTrigger(_shotgunDeath);
                    break;
                case DamageType.Spike:
                    _enemyAnimator.SetTrigger(_crossbowDeath);
                    break;
                case DamageType.SuperAcorn:
                    _enemyAnimator.SetTrigger(_superAcornDeath);
                    break;
            }
            
            // Disable the reaction layer when the enemy is dead
            _enemyAnimator.SetLayerWeight(ReactionLayerIndex, 0f);
        }
    }
}