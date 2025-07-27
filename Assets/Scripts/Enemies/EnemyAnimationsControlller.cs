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

        public void PlayTakeDamage(PlayerWeaponTypes damageType)
        {
            float layerWeight = (damageType == PlayerWeaponTypes.CACTUSSCROSSBOW) ? MediumLayerWeight : FullLayerWeight;
            _enemyAnimator.SetLayerWeight(ReactionLayerIndex, layerWeight);

            switch (damageType)
            {
                case PlayerWeaponTypes.ACORNGUN:
                    _enemyAnimator.SetTrigger(_acornReact);
                    break;
                case PlayerWeaponTypes.BANANASHOTGUN:
                    _enemyAnimator.SetTrigger(_shotgunReact);
                    break;
                case PlayerWeaponTypes.CACTUSSCROSSBOW:
                    _enemyAnimator.SetTrigger(_crossbowReact);
                    break;
            }
        }

        public void PlayStun()
        {
            _enemyAnimator.SetTrigger(_shotgunStun);
        }
        
        public void PlayDeath(PlayerWeaponTypes damageType)
        {
            _enemyAnimator.SetBool(_isDead, true);

            switch (damageType)
            {
                case PlayerWeaponTypes.ACORNGUN:
                    _enemyAnimator.SetTrigger(_acornDeath);
                    break;
                case PlayerWeaponTypes.BANANASHOTGUN:
                    _enemyAnimator.SetTrigger(_shotgunDeath);
                    break;
                case PlayerWeaponTypes.CACTUSSCROSSBOW:
                    _enemyAnimator.SetTrigger(_crossbowDeath);
                    break;
            }
            
            // Disable the reaction layer when the enemy is dead
            _enemyAnimator.SetLayerWeight(ReactionLayerIndex, 0f);
        }
    }
}