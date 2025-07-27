using UnityEngine;
using UnityEngine.AI;

namespace Enemies
{
    public class ChemAgentAnimationsController : EnemyAnimationsControlller
    {
        private readonly int _fire = Animator.StringToHash("Fire");
        
        public bool IsFiring
        {
            get => _enemyAnimator.GetBool(_fire);
            set => _enemyAnimator.SetBool(_fire, value);
        }
        
        public ChemAgentAnimationsController(Animator enemyAnimator, NavMeshAgent enemyAgent) : base(enemyAnimator, enemyAgent)
        {
        }
    }
}